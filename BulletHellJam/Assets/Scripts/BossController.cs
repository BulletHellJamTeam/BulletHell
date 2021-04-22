using UnityEngine;

public class BossController : MonoBehaviour {
	[SerializeField] private Animator animRef;
	private Rigidbody rigidBodyRef;

    public enum BossState { IDLE, ATTACKING, MOVING, DASHING };

	// timers
    private float horzTimeMin = 0.5f, horzTimeMax = 3f, horzTime = 0f;
    private float horzTimer = 0f;
    private float vertTimeMin = 2.5f, vertTimeMax = 4f, vertTime = 3f;
    private float vertTimer = 0f;

	// boundaries
	float LeftMoveBound, RightMoveBound, BottomMoveBound, TopMoveBound;

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

	// states
	private BossState state = BossState.MOVING;
	private BossState oldState = BossState.MOVING;

	private void Awake() {
		rigidBodyRef = GetComponent<Rigidbody>();
		targetXPos = rigidBodyRef.transform.position.x;
	}

	private void Start() {
		LeftMoveBound = Camera.main.ViewportToWorldPoint(new Vector3(0.4f, 0f, 0f)).x;
		RightMoveBound = Camera.main.ViewportToWorldPoint(new Vector3(0.95f, 0f, 0f)).x;

		BottomMoveBound = Camera.main.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).y;
		TopMoveBound = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, 0f)).y;

		minDist = (RightMoveBound - LeftMoveBound) / 5f;

		RandomizeTargetPosition();
	}

	private void FixedUpdate() {
		if (state != BossState.DASHING) {
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
				Dash();

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
}
