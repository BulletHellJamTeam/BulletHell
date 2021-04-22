using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour {

    public enum SpawnState { SPAWNING, WAITING, COUNTING, GAME_OVER };

    [SerializeField] private GameObject batPrefab;
    [SerializeField] private GameObject powerBatPrefab;
    private bool batSpawningComplete = false, powerBatSpawningComplete = false;

    [System.Serializable]
    public class Wave {
        public string name;
        public int batCount;
        public float batRate;
        public int powerBatCount;
        public float powerBatRate;
        public bool bossActive;
    }

    [SerializeField] private TMPro.TextMeshProUGUI waveText;
    [SerializeField] private Wave[] waves;
    [SerializeField] private Transform[] spawnPoints;

    private int nextWave = 0;
    private int bossStage = 1;
    private float waveTime = 2.5f, waveTimer = 2.5f;
    private SpawnState state = SpawnState.COUNTING;
    private float searchTime = 1f, searchTimer = 0f;

    private GameObject boss;
    private BossController bossController;

    private void Start() {
        boss = GameObject.FindGameObjectWithTag("Boss");
        bossController = boss.GetComponent<BossController>();
    }

    private void Update() {
        if (state == SpawnState.GAME_OVER) return;

        if (state == SpawnState.WAITING) {
            if (WaveComplete()) WaveCompleted(); else return;
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

        batSpawningComplete = false;
        powerBatSpawningComplete = false;

        if (wave.bossActive) bossController.Enter(bossStage++);

        StartCoroutine(SpawnBats(wave));
        StartCoroutine(SpawnPowerBats(wave));

        while (!batSpawningComplete && !powerBatSpawningComplete) yield return new WaitForSeconds(1f);

        state = SpawnState.WAITING;

        if (wave.bossActive) {
            if (wave.batRate != 0) {
                StartCoroutine(SpawnBatSummons(wave));
            }

            if (wave.powerBatRate != 0) {
                StartCoroutine(SpawnPowerBatSummons(wave));
            }
        }

        yield return null;
    }

    private IEnumerator SpawnBats(Wave wave) {
        for (int i=0; i<wave.batCount; i++) {
            SpawnEnemy(batPrefab.transform);
            yield return new WaitForSeconds(1f / wave.batRate);
        }

        batSpawningComplete = true;
        
        yield return null;
    }

    private IEnumerator SpawnPowerBats(Wave wave) {
        for (int i=0; i<wave.powerBatCount; i++) {
            SpawnEnemy(powerBatPrefab.transform);
            yield return new WaitForSeconds(1f / wave.powerBatRate);
        } 

        powerBatSpawningComplete = true;
        
        yield return null;
    }

    private IEnumerator SpawnBatSummons(Wave wave) {
        while (bossController.GetState() == BossController.BossState.STAGE1 
            || bossController.GetState() == BossController.BossState.STAGE2
            || bossController.GetState() == BossController.BossState.STAGE3
            || bossController.GetState() == BossController.BossState.ATTACK1
            || bossController.GetState() == BossController.BossState.ATTACK2) {
       
            SpawnEnemy(batPrefab.transform);
            yield return new WaitForSeconds(1f / wave.batRate);
        }
        
        yield return null;
    }

    private IEnumerator SpawnPowerBatSummons(Wave wave) {
        while (bossController.GetState() == BossController.BossState.STAGE1 
            || bossController.GetState() == BossController.BossState.STAGE2
            || bossController.GetState() == BossController.BossState.STAGE3
            || bossController.GetState() == BossController.BossState.ATTACK1
            || bossController.GetState() == BossController.BossState.ATTACK2) {
       
            SpawnEnemy(powerBatPrefab.transform);
            yield return new WaitForSeconds(1f / wave.powerBatRate);
        }
        
        yield return null;
    }

    private bool WaveComplete() {
        if (searchTimer > searchTime) {
            searchTimer = 0f;

            BossController.BossState bossState = bossController.GetState();
            if (bossState != BossController.BossState.IDLE && bossState != BossController.BossState.DEAD) return false;

            if (GameObject.FindGameObjectWithTag("Bat") != null) return false;
            if (GameObject.FindGameObjectWithTag("PowerBat") != null) return false;

            return true;
        } searchTimer += Time.deltaTime;

        return false;
    }

    private void WaveCompleted() {
        state = SpawnState.COUNTING;
        waveTimer = 0f;

        nextWave++;

        if (nextWave >= waves.Length) {
            state = SpawnState.GAME_OVER;
            waveText.text = "All waves defeated";
        }
    }

    private void SpawnEnemy(Transform enemy) {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        Instantiate(enemy, spawnPoint.position, Quaternion.identity);
    }
}
