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
        public List<EnemyGroup> enemyGroups; // List of enemy groups to spawn in this wave
        public int minEnemiesAlive; // Number of enemies to spawn in this wave
        public float spawnInterval; // Time between spawns in seconds
        public int spawnedCount; // Counter for already spawned enemies
    }

    [System.Serializable]
    public class EnemyGroup
    {
        public string enemyName;
        public int enemyCount; // Number of each enemy type to spawn
        public int spawnedCount; // The number of enemies of this type already spawned in the current wave
        public GameObject enemyPrefab;
    }

    public List<Wave> waves; // List of all the waves in the game
    public int currentWaveIndex = 0; // Index of the current wave

    [Header("Spawner Attributes")]
    float spawnTimer; // Timer to track time between spawns
    public int enemiesAlive; // Counter for currently alive enemies
    public int maxEnemiesAllowed; // Maximum number of enemies allowed on the map at once
    public bool maxEnemiesReached = false; // A flag to indicate if the maximum number of enemies has been reached
    public float waveInterval; // The interval between waves in seconds


    [Header("Spawn Positions")]
    public List<Transform> relativeSpawnPoints; // List of relative spawn points around the player


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

        if (currentWaveIndex < waves.Count - 1 && waveTimer >= waveInterval) // Check if the current wave has ended and the next wave can begin
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

        // If the minimum amount is not met, spawn until the minimum is met
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

    // Call the method when an enemy is killed
    public void OnEnemyKilled()
    {
        // Decrease the count of alive enemies
        enemiesAlive--;
        if(enemiesAlive < 0) enemiesAlive = 0;
    }
}
