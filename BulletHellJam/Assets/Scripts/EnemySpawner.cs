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

    public Wave[] waves;
    public Transform[] spawnPoints;

    private int nextWave = 0;
    private float timeBetweenWaves = 2.5f;
    private float waveCountdown;
    private SpawnState state = SpawnState.COUNTING;
    private float searchCountdown = 1f;

    private void Start() {
        waveCountdown = timeBetweenWaves;
    }

    private void Update() {
        if (state == SpawnState.GAME_OVER) return;

        if (state == SpawnState.WAITING) {
            if (!EnemyIsAlive()) {
                WaveCompleted();
            } else { 
                return;
            }
        }

        if (waveCountdown <= 0) {
            if (state != SpawnState.SPAWNING) {
                StartCoroutine(SpawnWave(waves[nextWave]));
            }
        } else {
            waveCountdown -= Time.deltaTime;
        }
    }

    private IEnumerator SpawnWave(Wave wave) {
        state = SpawnState.SPAWNING;

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
        state = SpawnState.COUNTING;
        waveCountdown = timeBetweenWaves;

        nextWave++;

        if (nextWave >= waves.Length)
        {
            state = SpawnState.GAME_OVER;
        }
    }

    private void SpawnEnemy(Transform enemy) {
        print("spawning enemy: " + enemy.name);

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        Instantiate(enemy, spawnPoint.position, Quaternion.identity);
    }
}
