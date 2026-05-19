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
    public bool boostedByCurse = true;

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

        if (HasWaveEnded())
        {
            AdvanceWave();
            return;
        }

        WaveData currentWave = data[currentWaveIndex];
        bool shouldRefillWave = currentWave.startingCount > 0 && EnemyStats.count < currentWave.startingCount;
        if (spawnTimer <= 0 || shouldRefillWave)
        {
            if (!CanSpawn())
            {
                ActiveCooldown();
                return;
            }

            GameObject[] spawns = currentWave.GetSpawns(EnemyStats.count);

            foreach (GameObject prefab in spawns)
            {
                if (!CanSpawn()) continue;

                Instantiate(prefab, GeneratePosition(), Quaternion.identity);
                currentWaveSpawnCount++;
            }

            ActiveCooldown();
        }
    }

    void AdvanceWave()
    {
        currentWaveIndex++;
        currentWaveDuration = currentWaveSpawnCount = 0;
        spawnTimer = 0f;

        if (currentWaveIndex >= data.Length)
        {
            Debug.Log("All waves completed!");
            enabled = false;
        }
    }

    public void ActiveCooldown()
    {
        float curseBoost = boostedByCurse ? GameManager.GetCumulativeCurse() : 1;
        spawnTimer += data[currentWaveIndex].GetSpawnInterval() / curseBoost;
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

        float x = Random.Range(0f, 1f);
        float y = Random.Range(0f, 1f);

        Vector3 position;

        switch (Random.Range(0, 2))
        {
            case 0:
            default:
                position = instance.referenceCamera.ViewportToWorldPoint(new Vector3(Mathf.Round(x), y, 10f));
                break;

            case 1:
                position = instance.referenceCamera.ViewportToWorldPoint(new Vector3(x, Mathf.Round(y), 10f));
                break;
        }

        position.z = 0f;
        return position;
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
