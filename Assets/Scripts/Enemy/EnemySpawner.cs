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
        public int waveQuota; // Number of enemies to spawn in this wave
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

    void Start()
    {
        player = FindAnyObjectByType<PlayerStats>().transform;
        CalculateWaveQuota();
    }

    void Update()
    {
        if (currentWaveIndex < waves.Count - 1 && waves[currentWaveIndex].spawnedCount == waves[currentWaveIndex].waveQuota) // Check if the current wave has ended and the next wave can begin
        {
            StartCoroutine(BeginNextWave());
        }

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= waves[currentWaveIndex].spawnInterval)
        {
            SpawnedEnemies();
            spawnTimer = 0f;
        }
    }

    IEnumerator BeginNextWave()
    {
        yield return new WaitForSeconds(waveInterval);

        if (currentWaveIndex < waves.Count - 1)
        {
            currentWaveIndex++;
            CalculateWaveQuota();
        }
    }

    void CalculateWaveQuota()
    {
        int currentWaveQuota = 0;
        foreach (EnemyGroup group in waves[currentWaveIndex].enemyGroups)
        {
            currentWaveQuota += group.enemyCount;
        }
        waves[currentWaveIndex].waveQuota = currentWaveQuota;
    }

    /// <summary>
    /// This method will stop spawning enemies if the amount of enemies on the map is maximum.
    /// The method will only spawn enemies in a particular wave until it is time for the next wave's enemies to be spawned.
    /// </summary>

    void SpawnedEnemies()
    {
        // Check if the minimum number of enemies for the current wave has been spawned
        if (waves[currentWaveIndex].spawnedCount < waves[currentWaveIndex].waveQuota && !maxEnemiesReached)
        {
            // Spawn each type of enemy in the current wave until the quota for that type is met
            foreach (EnemyGroup group in waves[currentWaveIndex].enemyGroups)
            {
                // Check if the minimum number of this type of enemy has been spawned
                if (group.spawnedCount < group.enemyCount)
                {
                    // Limit the number of enemis that can be spawned at once
                    if (enemiesAlive >= maxEnemiesAllowed)
                    {
                        maxEnemiesReached = true;
                        return;
                    }

                    //Spawn the enemy at a random spawn point around the player
                    Instantiate(group.enemyPrefab, player.position + relativeSpawnPoints[UnityEngine.Random.Range(0, relativeSpawnPoints.Count)].position, Quaternion.identity);

                    group.spawnedCount++;
                    waves[currentWaveIndex].spawnedCount++;
                    enemiesAlive++;
                }
            }
        }

        // Reset the flag if the number of alive enemies is below the maximum allowed
        if (enemiesAlive < maxEnemiesAllowed)
        {
            maxEnemiesReached = false;
        }
    }

    // Call the method when an enemy is killed
    public void OnEnemyKilled()
    {
        // Decrease the count of alive enemies
        enemiesAlive--;
    }
}
