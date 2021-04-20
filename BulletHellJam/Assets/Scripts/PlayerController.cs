using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
	private Rigidbody rigidBodyRef;
	[SerializeField] private Animator animRef;

	private Vector2 rawInputMovement;
	private float playerSpeed = 10f;

	private float LeftCamBound, RightCamBound, LeftWorldBound, RightWorldBound;
	private float BottomCamBound, TopCamBound, BottomWorldBound, TopWorldBound;

	private void Awake() {
		rigidBodyRef = gameObject.GetComponent<Rigidbody>();

		ComputeWorldBounds();
	}

	private void ComputeWorldBounds() {
		Bounds bounds = gameObject.transform.Find("PlayerMesh/pHairMirr").GetComponent<SkinnedMeshRenderer>().bounds;

		LeftWorldBound = Camera.main.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).x;
		RightWorldBound = Camera.main.ViewportToWorldPoint(new Vector3(1f, 0f, 0f)).x;

		LeftWorldBound += bounds.extents.x;
		RightWorldBound -= bounds.extents.x;

		LeftCamBound = Camera.main.WorldToViewportPoint(new Vector3(LeftWorldBound, 0f, 0f)).x;
		RightCamBound = Camera.main.WorldToViewportPoint(new Vector3(RightWorldBound, 0f, 0f)).x;

		BottomWorldBound = Camera.main.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).y;
		TopWorldBound = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, 0f)).y;

		//BottomWorldBound += bounds.extents.y;
		TopWorldBound -= 3.7f * bounds.extents.y;

		BottomCamBound = Camera.main.WorldToViewportPoint(new Vector3(0f, BottomWorldBound, 0f)).y;
		TopCamBound = Camera.main.WorldToViewportPoint(new Vector3(0f, TopWorldBound, 0f)).y;

	}

	private void Update() {
		Vector3 inputMovement = rawInputMovement.normalized * playerSpeed * Time.deltaTime;
		Vector3 new_pos = rigidBodyRef.transform.position + inputMovement;

		Vector3 pos = Camera.main.WorldToViewportPoint(new_pos);

		if(pos.x < LeftCamBound) {
			new_pos.x = LeftWorldBound;
		} else if(pos.x > RightCamBound) {
			new_pos.x = RightWorldBound;
		}
		
		if(pos.y < BottomCamBound) {
			new_pos.y = BottomWorldBound;
		} else if(pos.y > TopCamBound) {
			new_pos.y = TopWorldBound;
		}

		rigidBodyRef.MovePosition(new_pos);
	}

	public void OnMovement(InputAction.CallbackContext value) {
		rawInputMovement = value.ReadValue<Vector2>();
	}
}
