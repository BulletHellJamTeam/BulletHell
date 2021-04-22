using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
	[SerializeField] private Animator animRef;
	private Rigidbody rigidBodyRef;

	private Vector2 rawInputMovement;
	private Vector2 rawMousePosition;
	private float playerSpeed = 10f;

	private float maxSpeed = 10f;
	private float slowdownDuration = 0.5f;
	private float slowdownTimeElapsed = 0f;

	private void Awake() {
		rigidBodyRef = gameObject.GetComponent<Rigidbody>();
	}

	private void FixedUpdate() {
		Vector3 inputMovement = rawInputMovement.normalized * playerSpeed;

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
	}

	public void OnMovement(InputAction.CallbackContext value) {
		rawInputMovement = value.ReadValue<Vector2>();
	}

	public void OnMouse(InputAction.CallbackContext value) {
		rawMousePosition = value.ReadValue<Vector2>();
	}

	public void OnLeftClick(InputAction.CallbackContext value) {
	}

	public void OnRightClick(InputAction.CallbackContext value) {
	}

	public void OnDash(InputAction.CallbackContext value) {
	}
}
