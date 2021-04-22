using UnityEngine;

public class BossController : MonoBehaviour {
	// states
    public enum BossState { 
		IDLE, DEAD,
		STAGE1, STAGE2, STAGE3, 
		DASHING, RETREATING
	};

	private BossState state = BossState.IDLE;
	private BossState oldState = BossState.IDLE;

	// stats
	private float health1 = 300f, health2 = 300f, health3 = 300f;

	// references
	[SerializeField] private Animator animRef;
	[SerializeField] private Transform idlePosition;
	private Rigidbody rigidBodyRef;

	// timers
    private float horzTimeMin = 0.5f, horzTimeMax = 3f, horzTime = 0f;
    private float horzTimer = 0f;
    private float vertTimeMin = 2.5f, vertTimeMax = 4f, vertTime = 3f;
    private float vertTimer = 0f;

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
	}

	private void Start() {
		LeftMoveBound = Camera.main.ViewportToWorldPoint(new Vector3(0.4f, 0f, 0f)).x;
		RightMoveBound = Camera.main.ViewportToWorldPoint(new Vector3(0.95f, 0f, 0f)).x;

		minDist = (RightMoveBound - LeftMoveBound) / 5f;

		RandomizeTargetPosition();
	}

	private void FixedUpdate() {
		if (state == BossState.IDLE || state == BossState.DEAD) return; // if idle or dead, do nothing

		if (state == BossState.RETREATING) {
			rigidBodyRef.velocity = Vector3.zero;
			rigidBodyRef.MovePosition(idlePosition.position);

			state = BossState.IDLE;

			return; 
		}

		if (state == BossState.STAGE1 || state == BossState.STAGE2 || state == BossState.STAGE3) {
			if (state == BossState.DASHING) return;

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
        }

		if (state == BossState.DASHING) {
			horzTimer += Time.fixedDeltaTime;

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

			if (state == BossState.STAGE1) { 
				health1 -= pbm.GetDamage();

				if (health1 < 0f) state = BossState.RETREATING;
			} else if (state == BossState.STAGE2) { 
				health2 -= pbm.GetDamage();

				if (health2 < 0f) state = BossState.RETREATING;
			} else if (state == BossState.STAGE3) {
				health3 -= pbm.GetDamage();

				if (health3 < 0f) {
					state = BossState.DEAD;
					gameObject.SetActive(false);
				}

			} else { return; } // dont take damage unless we're in a stage!

            pbm.Destroy();
        }
    }
}
