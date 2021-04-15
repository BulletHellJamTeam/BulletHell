using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
	private Rigidbody2D rigidBodyRef;

	private Vector2 rawInputMovement;
	private float playerSpeed = 10f;

	private void Awake() {
		rigidBodyRef = gameObject.GetComponent<Rigidbody2D>();
	}

	private void FixedUpdate() {
		Vector3 inputMovement = rawInputMovement.normalized * playerSpeed * Time.fixedDeltaTime;
		rigidBodyRef.MovePosition(rigidBodyRef.transform.position + inputMovement);
	}

	public void OnMovement(InputAction.CallbackContext value) {
		rawInputMovement = value.ReadValue<Vector2>();
	}
}
