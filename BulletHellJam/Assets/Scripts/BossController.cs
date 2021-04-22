using UnityEngine;

public class BossController : MonoBehaviour {
	[SerializeField] private Animator animRef;
	private Rigidbody rigidBodyRef;

    public enum BossState { IDLE, ATTACKING, MOVING };

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
		}

		horzTimer += Time.fixedDeltaTime;
	}

	private void RandomizeTargetPosition() {
		targetXPos = Random.Range(LeftMoveBound, RightMoveBound);

		while (Mathf.Abs(targetXPos-rigidBodyRef.transform.position.x) < minDist) {
			targetXPos = Random.Range(LeftMoveBound, RightMoveBound);
		} 
	}
}
