using UnityEngine;

public class GameManager : MonoBehaviour {
	[SerializeField] private int targetFPS = 1000;
	[SerializeField] private GameObject playerRef;
	[Header("Boundaries")]
	[SerializeField] private GameObject LeftBound;
	[SerializeField] private GameObject RightBound;
	[SerializeField] private GameObject BottomBound;
	[SerializeField] private GameObject TopBound;

	private void Awake() {
		Physics.gravity = Vector3.zero;
		Application.targetFrameRate = targetFPS; // pali was here

		SetBoundaries();
	}

	private void SetBoundaries() {
		float LeftWorldBound = Camera.main.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).x;
		float RightWorldBound = Camera.main.ViewportToWorldPoint(new Vector3(1f, 0f, 0f)).x;

		float BottomWorldBound = Camera.main.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).y;
		float TopWorldBound = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, 0f)).y;

		LeftBound.transform.position = new Vector3(LeftWorldBound, 0f, 0f);
		LeftBound.transform.localScale = new Vector3(0.25f, TopWorldBound - BottomWorldBound, 1f);

		RightBound.transform.position = new Vector3(RightWorldBound, 0f, 0f);
		RightBound.transform.localScale = new Vector3(0.25f, TopWorldBound - BottomWorldBound, 1f);

		BottomBound.transform.position = new Vector3(0f, BottomWorldBound, 0f);
		BottomBound.transform.localScale = new Vector3(RightWorldBound - LeftWorldBound, 0.25f, 1f);

		TopBound.transform.position = new Vector3(0f, TopWorldBound, 0f);
		TopBound.transform.localScale = new Vector3(RightWorldBound - LeftWorldBound, 0.25f, 1f);
	}
}
