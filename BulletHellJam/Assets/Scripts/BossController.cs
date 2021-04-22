using UnityEditor.SceneManagement;
using UnityEngine;

public class BossController : MonoBehaviour {
	// states
    public enum BossState { 
		IDLE, DEAD,
		STAGE1, STAGE2, STAGE3, 
		ATTACK1, ATTACK2,
		DASHING, RETREATING
	};

	private BossState state = BossState.IDLE;
	private BossState oldState = BossState.IDLE;

	// stats
	private float health1 = 300f, health2 = 300f, health3 = 300f;

	// references
	[SerializeField] private Animator animRef;
	[SerializeField] private Transform idlePosition;
	[SerializeField] private GameObject explosion;
    [SerializeField] private Material damageMaterial;
    [SerializeField] private GameObject hair;
    [SerializeField] private GameObject body;
    private Material originalHairMaterial;
    private Material originalBodyMaterial;
	private Rigidbody rigidBodyRef;

	// movement timers
    private float horzTimeMin = 0.5f, horzTimeMax = 3f, horzTime = 0f;
    private float horzTimer = 0f;
    private float vertTimeMin = 2.5f, vertTimeMax = 4f, vertTime = 3f;
    private float vertTimer = 0f;

	// attack timers
    private float attackTimeMin = 3.5f, attackTimeMax = 6.5f, attackTime = 5f;
    private float attackTimer = 0f;
    private float attack1DurationTime = 2f; private float attack1DurationTimer = 0f;
    private float attack2DurationTime = 3f; private float attack2DurationTimer = 0f;
	private float attack1Prob = 0.7f;

	// boundaries
	float LeftMoveBound, RightMoveBound;

	// movement data
	private float bossSpeed = 3f, maxSpeed = 10f;
	private float targetXPos;
	private float minDist;

	// slowdown data
	private float slowdownTime = 1f, slowdownTimer = 0f;
	private bool slowingDown = false;

	// dashing variables
	[SerializeField] private GameObject echoPrefab;
	private float dashSpeed = 50f, dashSpeedMin = 50f, dashSpeedMax = 100f;
	private float dashTime = 0.05f, dashTimer = 0f;
	private float dashEchoTime = 0.001f, dashEchoTimer = 0f;

	private void Awake() {
		rigidBodyRef = GetComponent<Rigidbody>();
		targetXPos = rigidBodyRef.transform.position.x;

        originalHairMaterial = hair.GetComponent<SkinnedMeshRenderer>().material;
        originalBodyMaterial = body.GetComponent<SkinnedMeshRenderer>().material;
	}

	private void Start() {
		LeftMoveBound = Camera.main.ViewportToWorldPoint(new Vector3(0.4f, 0f, 0f)).x;
		RightMoveBound = Camera.main.ViewportToWorldPoint(new Vector3(0.95f, 0f, 0f)).x;

		minDist = (RightMoveBound - LeftMoveBound) / 5f;

		RandomizeTargetPosition();
	}

	private void FixedUpdate() {
		if (state == BossState.IDLE || state == BossState.DEAD) return; // if idle or dead, do nothing

		print(state);

		if (state == BossState.RETREATING) {
			rigidBodyRef.velocity = Vector3.zero;
			rigidBodyRef.MovePosition(idlePosition.position);

			state = BossState.IDLE;

			return;
		}

		if (state == BossState.STAGE1 || state == BossState.STAGE2 || state == BossState.STAGE3) {
			Vector3 rawMovement = new Vector3(targetXPos - rigidBodyRef.transform.position.x, 0f, 0f);
			Vector3 movement = rawMovement.normalized * bossSpeed;

    		if (rawMovement.magnitude > minDist && !slowingDown) { 
    			rigidBodyRef.AddForce(movement);
    		} else {
    			slowingDown = true;

    			if (slowdownTimer < slowdownTime) {
    				rigidBodyRef.velocity = Vector3.Lerp(rigidBodyRef.velocity, Vector3.zero, slowdownTimer / slowdownTime);
    				slowdownTimer += Time.fixedDeltaTime;
    			} else {
    				slowdownTimer = 0f;
    			}
    		}

    		if(rigidBodyRef.velocity.magnitude > maxSpeed){
              rigidBodyRef.velocity = Vector3.ClampMagnitude(rigidBodyRef.velocity, maxSpeed);
         }
    
    		animRef.SetFloat("VelocityX", rigidBodyRef.velocity.x);
    		animRef.SetFloat("VelocityY", rigidBodyRef.velocity.y);
    
    		if (horzTimer > horzTime) {
    			slowingDown = false;
    			RandomizeTargetPosition();
    
    			horzTimer = 0f;
    			horzTime = Random.Range(horzTimeMin, horzTimeMax);
    
    			slowdownTimer = 0f;
    		} horzTimer += Time.fixedDeltaTime;
		
			if (vertTimer > vertTime) {
				if (rigidBodyRef.position.x < RightMoveBound) Dash();

    			vertTimer = 0f;
    			vertTime = Random.Range(vertTimeMin, vertTimeMax);
    		} vertTimer += Time.fixedDeltaTime;

			if (attackTimer > attackTime) {
				if (rigidBodyRef.position.x < RightMoveBound) {
					if (state == BossState.STAGE1) Attack1();
					else if (state == BossState.STAGE2) Attack2();
					else if (state == BossState.STAGE3) {
						if (Random.Range(0f, 1f) > attack1Prob) Attack1();
						else Attack2();
					}
				}

    			attackTimer = 0f;
    			attackTime = Random.Range(attackTimeMin, attackTimeMax);
    		} attackTimer += Time.fixedDeltaTime;

			return;
        }

		if (state == BossState.DASHING || state == BossState.ATTACK1 || state == BossState.ATTACK2) {
    		animRef.SetFloat("VelocityX", rigidBodyRef.velocity.x);
    		animRef.SetFloat("VelocityY", rigidBodyRef.velocity.y);
		}

		if (state == BossState.DASHING) {
			dashTimer += Time.fixedDeltaTime;
			dashEchoTimer += Time.fixedDeltaTime;

		    if (dashEchoTimer > dashEchoTime) {
		    	GameObject echo = (GameObject)Instantiate(echoPrefab, rigidBodyRef.transform.position, Quaternion.identity);
		    	Destroy(echo, 0.5f);

		    	dashEchoTimer = 0f;
		    }
			
			if (dashTimer > dashTime) {
		    	rigidBodyRef.velocity = Vector3.zero;

		    	state = oldState;
		    }

			return;
        }

		if (state == BossState.ATTACK1) {
			attack1DurationTimer += Time.fixedDeltaTime;

			rigidBodyRef.velocity = Vector3.zero;

			// perform attack pattern
			// print("attack1");
			
			if (attack1DurationTimer > attack1DurationTime) {
		    	state = oldState;

				attack1DurationTimer = 0f;
		    }

			return;
		}

		if (state == BossState.ATTACK2) {
			attack2DurationTimer += Time.fixedDeltaTime;

			rigidBodyRef.velocity = Vector3.zero;

			// perform attack pattern
			// print("attack2");
			
			if (attack2DurationTimer > attack2DurationTime) {
		    	state = oldState;

				attack2DurationTimer = 0f;
		    }

			return;
		}
	}

	private void RandomizeTargetPosition() {
		targetXPos = Random.Range(LeftMoveBound, RightMoveBound);

		while (Mathf.Abs(targetXPos-rigidBodyRef.transform.position.x) < minDist) {
			targetXPos = Random.Range(LeftMoveBound, RightMoveBound);
		} 
	}

	private void Dash() {
		dashTimer = 0f;
		dashEchoTimer = 0f;

		oldState = state;
		state = BossState.DASHING;

		dashSpeed = Random.Range(dashSpeedMin, dashSpeedMax);

		if (Random.Range(0, 2) == 1f) { 
			rigidBodyRef.velocity += Vector3.up * dashSpeed;
		} else {
			rigidBodyRef.velocity = Vector3.down * dashSpeed;
		}
	}

	private void Attack1() {
		oldState = state;
		state = BossState.ATTACK1;

		// do attack stuff, like run a coroutine or something
	}

	private void Attack2() {
		oldState = state;
		state = BossState.ATTACK2;

		// do attack stuff, like run a coroutine or something
	}

	public BossState GetState() { return state; }

	public void Enter(int stage) {
		if (stage == 1) {
			state = BossState.STAGE1;
			oldState = BossState.STAGE1;
		} else if (stage == 2) {
			state = BossState.STAGE2;
			oldState = BossState.STAGE2;
		} else if (stage == 3) {
			state = BossState.STAGE3;
			oldState = BossState.STAGE3;
		}
	}

    public void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("PlayerBullet")) {
			PlayerBulletManager pbm = other.gameObject.GetComponent<PlayerBulletManager>();

			if (state == BossState.STAGE1 || oldState == BossState.STAGE1) { 
				health1 -= pbm.GetDamage();

				if (health1 < 0f) {
					state = BossState.RETREATING;

					GameObject exp = Instantiate(explosion, transform.position, Quaternion.identity);
					Destroy(exp, 0.5f);
				} else {
					hair.GetComponent<SkinnedMeshRenderer>().material = damageMaterial;
					body.GetComponent<SkinnedMeshRenderer>().material = damageMaterial;

					Invoke("ResetMaterial", 0.2f);
				}
			} else if (state == BossState.STAGE2 || oldState == BossState.STAGE2) { 
				health2 -= pbm.GetDamage();

				if (health2 < 0f) {
					state = BossState.RETREATING;

					GameObject exp = Instantiate(explosion, transform.position, Quaternion.identity);
					Destroy(exp, 0.5f);
				} else {
					hair.GetComponent<SkinnedMeshRenderer>().material = damageMaterial;
					body.GetComponent<SkinnedMeshRenderer>().material = damageMaterial;

					Invoke("ResetMaterial", 0.2f);
				}
			} else if (state == BossState.STAGE3 || oldState == BossState.STAGE3) {
				health3 -= pbm.GetDamage();

				if (health3 < 0f) {
					state = BossState.DEAD;
					gameObject.SetActive(false);

					GameObject exp = Instantiate(explosion, transform.position, Quaternion.identity);
					Destroy(exp, 0.5f);
				} else {
					hair.GetComponent<SkinnedMeshRenderer>().material = damageMaterial;
					body.GetComponent<SkinnedMeshRenderer>().material = damageMaterial;

					Invoke("ResetMaterial", 0.2f);
				}
			} else { return; } // dont take damage unless we're in a stage!

		    pbm.Destroy();
		}
    }

    private void ResetMaterial() {
        hair.GetComponent<SkinnedMeshRenderer>().material = originalHairMaterial;
        body.GetComponent<SkinnedMeshRenderer>().material = originalBodyMaterial;
    }
}
