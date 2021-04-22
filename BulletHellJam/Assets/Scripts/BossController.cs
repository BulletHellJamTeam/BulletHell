using UnityEngine;

public class BossController : MonoBehaviour {
	[SerializeField] private Animator animRef;
	private Rigidbody rigidBodyRef;

    public enum BossState { IDLE, ATTACKING, MOVING };

    private float horzTimeMin = 0.5f, horzTimeMax = 3f, horzTime = 0f;
    private float horzTimer = 0f;
    private float vertTimeMin = 0.2f, vertTimeMax = 1.5f, vertTime = 3f;
    private float vertTimer = 0f;

	float targetXPos;

	float LeftMoveBound, RightMoveBound, BottomMoveBound, TopMoveBound;

	private float bossSpeed = 3f;

	private float maxSpeed = 10f;
	private bool slowingDown = false;
	private float slowdownDuration = 1f;
	private float slowdownTimeElapsed = 0f;
	private float min_dist;

	private void Awake() {
		rigidBodyRef = GetComponent<Rigidbody>();
		targetXPos = rigidBodyRef.transform.position.x;
	}

	private void Start() {
		LeftMoveBound = Camera.main.ViewportToWorldPoint(new Vector3(0.4f, 0f, 0f)).x;
		RightMoveBound = Camera.main.ViewportToWorldPoint(new Vector3(0.95f, 0f, 0f)).x;

		BottomMoveBound = Camera.main.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).y;
		TopMoveBound = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, 0f)).y;

		min_dist = (RightMoveBound - LeftMoveBound) / 5f;

		RandomizeTargetPosition();
	}

	private void FixedUpdate() {
		Vector3 rawMovement = new Vector3(targetXPos - rigidBodyRef.transform.position.x, 0f, 0f);
		Vector3 movement = rawMovement.normalized * bossSpeed;

		if (rawMovement.magnitude > min_dist && !slowingDown) { 
			rigidBodyRef.AddForce(movement);
		} else {
			slowingDown = true;

			if (slowdownTimeElapsed < slowdownDuration) {
				rigidBodyRef.velocity = Vector3.Lerp(rigidBodyRef.velocity, Vector3.zero, slowdownTimeElapsed / slowdownDuration);
				slowdownTimeElapsed += Time.fixedDeltaTime;
			} else {
				slowdownTimeElapsed = 0f;
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
		}

		horzTimer += Time.fixedDeltaTime;
	}

	private void RandomizeTargetPosition() {
		targetXPos = Random.Range(LeftMoveBound, RightMoveBound);

		while (Mathf.Abs(targetXPos-rigidBodyRef.transform.position.x) < min_dist) {
			targetXPos = Random.Range(LeftMoveBound, RightMoveBound);
		} 
	}
}
