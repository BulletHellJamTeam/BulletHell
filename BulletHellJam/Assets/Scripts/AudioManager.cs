using UnityEngine;

public class AudioManager : MonoBehaviour {
	[SerializeField] private AudioSource StageTheme, BossTheme;

	public bool stage = true;

	private void Start() {
		PlayStage();
	}

	public void PlayStage() {
		BossTheme.Stop();
		StageTheme.Play();

		stage = true;
	}

	public void PlayBoss() {
		StageTheme.Stop();
		BossTheme.Play();

		stage = false;
	}
}
