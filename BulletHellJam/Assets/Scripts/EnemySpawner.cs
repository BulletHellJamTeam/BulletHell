using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour {

    public enum SpawnState { SPAWNING, WAITING, COUNTING };

    [System.Serializable]
    public class Wave {
        public string name;
        public Transform enemy;
        public int count;
        public float rate;
    }

    public Wave[] waves;

    private int nextWave = 0;
    private float timeBetweenWaves = 5f;
    private float waveCountdown;
    private SpawnState state = SpawnState.COUNTING;

    private void Start() {
        waveCountdown = timeBetweenWaves;
    }

    private void Update() {
        if (waveCountdown <= 0) {
            if (state != SpawnState.SPAWNING) {
                StartCoroutine(SpawnWave(waves[nextWave]));
            } else {
                waveCountdown -= Time.deltaTime;
            }
        }
    }

    IEnumerator SpawnWave(Wave wave) {
        state = SpawnState.SPAWNING;

        for (int i=0; i<wave.count; i++) {
            SpawnEnemy(wave.enemy);
            yield return new WaitForSeconds(1f / wave.rate);
        }

        yield break;
    }

    private void SpawnEnemy(Transform enemy) {
        Debug.Log("spawning enemy: " + enemy.name);
    }
}
