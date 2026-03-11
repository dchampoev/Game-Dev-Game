using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public string waveName;
        public List<EnemyGroup> enemyGroups;
        public int minEnemiesAlive;
        public float spawnInterval;
        public int spawnedCount;
    }

    [System.Serializable]
    public class EnemyGroup
    {
        public string enemyName;
        public int enemyCount;
        public int spawnedCount;
        public GameObject enemyPrefab;
    }

    public List<Wave> waves;
    public int currentWaveIndex = 0;

    [Header("Spawner Attributes")]
    float spawnTimer;
    public int enemiesAlive;
    public int maxEnemiesAllowed;
    public bool maxEnemiesReached = false;
    public float waveInterval;


    [Header("Spawn Positions")]
    public List<Transform> relativeSpawnPoints;


    Transform player;
    float waveTimer;

    void Start()
    {
        player = FindAnyObjectByType<PlayerStats>().transform;

        waveTimer = 0f;
        spawnTimer = 0f;

        ResetCurrentWaveCounts();

        SpawnedEnemies();
    }

    void Update()
    {
        if (!player) return;
        if(waves==null || waves.Count == 0) return;
        if(currentWaveIndex < 0 || currentWaveIndex >= waves.Count) return;

        waveTimer += Time.deltaTime;

        if (currentWaveIndex < waves.Count - 1 && waveTimer >= waveInterval)
        {
            waveTimer = 0f;
            currentWaveIndex++;
            ResetCurrentWaveCounts();
        }

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= waves[currentWaveIndex].spawnInterval)
        {
            SpawnedEnemies();
            spawnTimer = 0f;
        }
    }

    void ResetCurrentWaveCounts()
    {
        if(waves==null || waves.Count == 0) return;
        if(currentWaveIndex<0 || currentWaveIndex >= waves.Count) return;
        waves[currentWaveIndex].spawnedCount = 0;
        if (waves[currentWaveIndex].enemyGroups == null) waves[currentWaveIndex].enemyGroups = new List<EnemyGroup>();
        
        foreach (EnemyGroup group in waves[currentWaveIndex].enemyGroups)
        {
            group.spawnedCount = 0;
        }
    }

    /// <summary>
    /// This method will stop spawning enemies if the amount of enemies on the map is maximum.
    /// The method will only spawn enemies in a particular wave until it is time for the next wave's enemies to be spawned.
    /// </summary>

    void SpawnedEnemies()
    {
        if (!player) return;
        if (relativeSpawnPoints == null || relativeSpawnPoints.Count == 0) return;

        const int hardCap = 300;
        if (enemiesAlive >= hardCap) return;

        int allowedCap = maxEnemiesAllowed <= 0 ? hardCap : Mathf.Min(maxEnemiesAllowed, hardCap);

        if (enemiesAlive >= allowedCap)
        {
            maxEnemiesReached = true;
            return;
        }
        maxEnemiesReached = false;

        Wave wave = waves[currentWaveIndex];
        if (wave.enemyGroups == null || wave.enemyGroups.Count == 0) return;

        int spawnBudget = allowedCap - enemiesAlive;
        if (spawnBudget <= 0) return;

        int minimumAlive = Math.Max(0, wave.minEnemiesAlive);

        if (enemiesAlive < minimumAlive)
        {
            int deficit = minimumAlive - enemiesAlive;
            int toSpawn = Math.Min(deficit, spawnBudget);

            for (int i = 0; i < toSpawn; i++)
            {
                EnemyGroup groupToSpawn = wave.enemyGroups[UnityEngine.Random.Range(0, wave.enemyGroups.Count)];
                if (!groupToSpawn.enemyPrefab) continue;

                Transform spawnPoint = relativeSpawnPoints[UnityEngine.Random.Range(0, relativeSpawnPoints.Count)];
                Instantiate(groupToSpawn.enemyPrefab, player.position + spawnPoint.position, Quaternion.identity);

                groupToSpawn.spawnedCount++;
                wave.spawnedCount++;
                enemiesAlive++;
            }

            return;
        }
        // If more enemies than the minimum amount are present, spawn one of each type
        int typesToSpawn = Mathf.Min(wave.enemyGroups.Count, spawnBudget);

        // Shuffle the indices so "one of each type" isn't always in the same order.
        List<int> indices = new List<int>(wave.enemyGroups.Count);
        for (int i = 0; i < wave.enemyGroups.Count; i++)
        {
            indices.Add(i);
        }

        for (int i = 0; i < typesToSpawn; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, indices.Count);
            int temp = indices[i];
            indices[i] = indices[randomIndex];
            indices[randomIndex] = temp;

            EnemyGroup groupToSpawn = wave.enemyGroups[indices[i]];
            if (!groupToSpawn.enemyPrefab) continue;

            Transform spawnPoint = relativeSpawnPoints[UnityEngine.Random.Range(0, relativeSpawnPoints.Count)];
            Instantiate(groupToSpawn.enemyPrefab, player.position + spawnPoint.position, Quaternion.identity);

            groupToSpawn.spawnedCount++;
            wave.spawnedCount++;
            enemiesAlive++;
        }
    }

    public void OnEnemyKilled()
    {
        enemiesAlive--;
        if(enemiesAlive < 0) enemiesAlive = 0;
    }
}
