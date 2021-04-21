using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour {
    private float timeBetweenWaves = 5f;
    private int currentWave;

    private void Start() {
        currentWave = 1;
    }
}
