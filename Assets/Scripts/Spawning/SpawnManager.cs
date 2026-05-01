using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    int currentWaveIndex;
    int currentWaveSpawnCount = 0;

    public WaveData[] data;
    public Camera referenceCamera;

    [Tooltip("If there are more than than this number of enemies, stop spawning any more")]
    public int maxEnemiesAllowed = 300;
    float spawnTimer;
    float currentWaveDuration = 0f;

    public static SpawnManager instance;

    void Start()
    {
        if (instance) Debug.LogWarning("There is more than one Spawn Manager in the scene! Please remove the extra Spawn Managers.");
        instance = this;
    }

    void Update()
    {
        spawnTimer -= Time.deltaTime;
        currentWaveDuration += Time.deltaTime;

        if (spawnTimer <= 0)
        {
            if (HasWaveEnded())
            {
                currentWaveIndex++;
                currentWaveDuration = currentWaveSpawnCount = 0;

                if (currentWaveIndex >= data.Length)
                {
                    Debug.Log("All waves completed!");
                    enabled = false;
                }

                return;
            }

            if (!CanSpawn())
            {
                spawnTimer += data[currentWaveIndex].GetSpawnInterval();
                return;
            }

            GameObject[] spawns = data[currentWaveIndex].GetSpawns(EnemyStats.count);

            foreach (GameObject prefab in spawns)
            {
                if (!CanSpawn()) continue;

                Instantiate(prefab, GeneratePosition(), Quaternion.identity);
                currentWaveSpawnCount++;
            }

            spawnTimer += data[currentWaveIndex].GetSpawnInterval();
        }
    }

    public bool CanSpawn()
    {
        if (HasExceededTotalSpawns()) return false;

        WaveData currentWave = data[currentWaveIndex];

        if (currentWaveSpawnCount >= currentWave.totalSpawns)
            return false;

        return true;
    }

    public static bool HasExceededTotalSpawns()
    {
        if (!instance) return false;
        if (EnemyStats.count > instance.maxEnemiesAllowed) return true;
        return false;
    }

    public bool HasWaveEnded()
    {
        WaveData currentWave = data[currentWaveIndex];

        if ((currentWave.exitConditions & WaveData.ExitCondition.waveDuration) > 0)
        {
            if (currentWaveDuration < currentWave.duration) return false;
        }

        if ((currentWave.exitConditions & WaveData.ExitCondition.reachedTotalSpawns) > 0)
        {
            if (currentWaveSpawnCount < currentWave.totalSpawns) return false;
        }

        if (currentWave.mustKillAllEnemies && EnemyStats.count > 0)
        {
            return false;
        }

        return true;
    }

    void Reset()
    {
        referenceCamera = Camera.main;
    }

    public static Vector3 GeneratePosition()
    {
        if (!instance.referenceCamera) instance.referenceCamera = Camera.main;

        if (!instance.referenceCamera.orthographic)
        {
            Debug.LogWarning("Spawn Manager's reference camera is not orthographic! Defaulting to (0, 0, 0) for spawn position.");
        }

        float x = Random.Range(0f, 1f), y = Random.Range(0f, 1f);

        switch (Random.Range(0, 2))
        {
            case 0:
            default:
                return instance.referenceCamera.ViewportToWorldPoint(new Vector3(Mathf.Round(x), y, 0f));
            case 1:
                return instance.referenceCamera.ViewportToWorldPoint(new Vector3(x, Mathf.Round(y), 0f));
        }
    }

    public static bool IsWithinBoundaries(Transform checkedObj)
    {
        Camera camera = instance && instance.referenceCamera ? instance.referenceCamera : Camera.main;

        Vector2 viewport = camera.WorldToViewportPoint(checkedObj.position);
        if (viewport.x < 0f || viewport.x > 1f) return false;
        if (viewport.y < 0f || viewport.y > 1f) return false;
        return true;
    }
}
