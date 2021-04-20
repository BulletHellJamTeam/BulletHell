using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
	private Rigidbody rigidBodyRef;
	[SerializeField] private Animator animRef;

	private Vector2 rawInputMovement;
	private float playerSpeed = 10f;

	private float maxSpeed = 10f;
	private float slowdownDuration = 0.5f;
	private float slowdownTimeElapsed = 0f;

	private float LeftCamBound, RightCamBound, LeftWorldBound, RightWorldBound;
	private float BottomCamBound, TopCamBound, BottomWorldBound, TopWorldBound;

	private void Awake() {
		rigidBodyRef = gameObject.GetComponent<Rigidbody>();
	}

	private void FixedUpdate() {
		Vector3 inputMovement = rawInputMovement.normalized * playerSpeed;

		print(inputMovement);

		rigidBodyRef.AddForce(inputMovement);

		if(rigidBodyRef.velocity.magnitude > maxSpeed){
             rigidBodyRef.velocity = Vector3.ClampMagnitude(rigidBodyRef.velocity, maxSpeed);
        }

		if (inputMovement == Vector3.zero && slowdownTimeElapsed < slowdownDuration) {
			rigidBodyRef.velocity = Vector3.Lerp(rigidBodyRef.velocity, Vector3.zero, slowdownTimeElapsed / slowdownDuration);
			slowdownTimeElapsed += Time.fixedDeltaTime;
		} else {
			slowdownTimeElapsed = 0f;
		}

		animRef.SetFloat("VelocityX", rigidBodyRef.velocity.x);
		animRef.SetFloat("VelocityY", rigidBodyRef.velocity.y);

		//print(rigidBodyRef.velocity);
	}

	public void OnMovement(InputAction.CallbackContext value) {
		rawInputMovement = value.ReadValue<Vector2>();
	}
}
