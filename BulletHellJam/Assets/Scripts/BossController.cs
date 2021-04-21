using UnityEngine;

public class BossController : MonoBehaviour {
	[SerializeField] private Animator animRef;
	private Rigidbody rigidBodyRef;

    public enum BossState { IDLE, ATTACKING, MOVING };

	float time;
	float updateTime = 5f;

	Vector3 targetPos;

	float LeftMoveBound, RightMoveBound, BottomMoveBound, TopMoveBound;

	private float bossSpeed = 3f;

	private float maxSpeed = 10f;
	private bool slowingDown = false;
	private bool recalculatingPos = true;
	private float slowdownDuration = 0.5f;
	private float slowdownTimeElapsed = 0f;
	private float min_dist;

	private void Awake() {
		rigidBodyRef = GetComponent<Rigidbody>();
		targetPos = rigidBodyRef.transform.position;
	}

	private void Start() {
		LeftMoveBound = Camera.main.ViewportToWorldPoint(new Vector3(0.55f, 0f, 0f)).x;
		RightMoveBound = Camera.main.ViewportToWorldPoint(new Vector3(1f, 0f, 0f)).x;

		BottomMoveBound = Camera.main.ViewportToWorldPoint(new Vector3(0f, 0.55f, 0f)).y;
		TopMoveBound = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, 0f)).y;

		min_dist = (RightMoveBound - LeftMoveBound) / 10f;

		RandomizeTargetPosition();

		time = 0;
	}

	private void FixedUpdate() {
		Vector3 rawMovement = targetPos - rigidBodyRef.transform.position;
		Vector3 movement = rawMovement.normalized * bossSpeed;

		//print(rigidBodyRef.transform.position + " - " + targetPos);

		if (rawMovement.magnitude > min_dist && !slowingDown) { 
			if (!recalculatingPos) rigidBodyRef.AddForce(movement);
		} else {
			slowingDown = true;

			if (slowdownTimeElapsed < slowdownDuration) {
				rigidBodyRef.velocity = Vector3.Lerp(rigidBodyRef.velocity, Vector3.zero, slowdownTimeElapsed / slowdownDuration);
				slowdownTimeElapsed += Time.fixedDeltaTime;
			} else {
				slowdownTimeElapsed = 0f;
			}
		}

		if (recalculatingPos) rigidBodyRef.velocity = Vector3.zero;
		print(rigidBodyRef.velocity);

		if(rigidBodyRef.velocity.magnitude > maxSpeed){
             rigidBodyRef.velocity = Vector3.ClampMagnitude(rigidBodyRef.velocity, maxSpeed);
        }

		animRef.SetFloat("VelocityX", rigidBodyRef.velocity.x);
		animRef.SetFloat("VelocityY", rigidBodyRef.velocity.y);

		if (time > updateTime) {
			slowingDown = false;
			RandomizeTargetPosition();

			time = 0f;
		}

		time += Time.fixedDeltaTime;
	}

	private void RandomizeTargetPosition() {
		targetPos = new Vector3(Random.Range(LeftMoveBound, RightMoveBound), targetPos.y, targetPos.z);

		recalculatingPos = true;

		while ((targetPos-rigidBodyRef.transform.position).magnitude < min_dist) {
		 	print((targetPos - rigidBodyRef.transform.position).magnitude);

		 	targetPos = new Vector3(Random.Range(LeftMoveBound, RightMoveBound), targetPos.y, targetPos.z);
		} 

		recalculatingPos = false;
	}
}
