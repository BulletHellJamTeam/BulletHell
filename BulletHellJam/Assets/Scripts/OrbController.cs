using UnityEngine;

public class OrbController : MonoBehaviour {
	private Rigidbody rigidbodyRef;
	private GameObject playerRef;

	private float orbSpeed = 50f;
	private float frequency = 0.3f;
	private float amplitude = 0.01f;

	private void Awake() {
		rigidbodyRef = GetComponent<Rigidbody>();
		playerRef = GameObject.Find("Player");
	}

	private void FixedUpdate() {
		Vector3 dist = playerRef.transform.position - rigidbodyRef.transform.position;

		if (dist.magnitude < 3f) {
			rigidbodyRef.AddForce(dist.normalized * orbSpeed * Time.fixedDeltaTime);
		} else {
			rigidbodyRef.velocity = Vector3.zero;

		    Vector3 pos = rigidbodyRef.transform.position;
			pos.y += Mathf.Sin (Time.fixedTime * Mathf.PI * frequency) * amplitude;

			rigidbodyRef.MovePosition(pos);

		}
	}
}
