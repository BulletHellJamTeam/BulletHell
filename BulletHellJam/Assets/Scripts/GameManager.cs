using UnityEngine;

public class GameManager : MonoBehaviour {

	public int targetFPS = 1000;
	private void Awake() {
		Physics.gravity = Vector3.zero;
		Application.targetFrameRate = targetFPS; // pali was here
	}
}
