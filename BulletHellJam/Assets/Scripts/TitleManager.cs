using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour {
	[SerializeField] private Button StartButton;

	private void Awake() {
		StartButton.onClick.AddListener(OnStartButton);
	}

	void OnStartButton() {
		SceneManager.LoadScene("MainScene");
	}
}
