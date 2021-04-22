using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour {

    public enum SpawnState { SPAWNING, WAITING, COUNTING, GAME_OVER };

    [System.Serializable]
    public class Wave {
        public string name;
        public Transform enemy;
        public int count;
        public float rate;
    }

    [SerializeField] private TMPro.TextMeshProUGUI waveText;

    public Wave[] waves;
    public Transform[] spawnPoints;

    private int nextWave = 0;
    private float waveTime = 2.5f, waveTimer = 2.5f;
    private SpawnState state = SpawnState.COUNTING;
    private float searchCountdown = 1f;

    private void Update() {
        if (state == SpawnState.GAME_OVER) return;

        if (state == SpawnState.WAITING) {
            if (!EnemyIsAlive()) {
                WaveCompleted();
            } else { 
                return;
            }
        }

        if (waveTimer > waveTime) {
            if (state != SpawnState.SPAWNING) {
                StartCoroutine(SpawnWave(waves[nextWave]));
            }
        }

        waveTimer += Time.deltaTime;
    }

    private IEnumerator SpawnWave(Wave wave) {
        state = SpawnState.SPAWNING;

        waveText.text = wave.name;

        for (int i=0; i<wave.count; i++) {
            SpawnEnemy(wave.enemy);
            yield return new WaitForSeconds(1f / wave.rate);
        }

        yield break;
    }

    private bool EnemyIsAlive() {
        searchCountdown -= Time.deltaTime;
        if (searchCountdown <= 0f) {
            searchCountdown = 1f;
            if (GameObject.FindGameObjectWithTag("Bat") == null) {
                return false;
            }
        } return true;

    }

    private void WaveCompleted() {
        print("wave completed");

        state = SpawnState.COUNTING;
        waveTimer = 0f;

        nextWave++;

        if (nextWave >= waves.Length) {
            state = SpawnState.GAME_OVER;
        }
    }

    private void SpawnEnemy(Transform enemy) {
        print("spawning enemy: " + enemy.name);

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        Instantiate(enemy, spawnPoint.position, Quaternion.identity);
    }
}
