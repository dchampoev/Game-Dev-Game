using UnityEngine;

[CreateAssetMenu(fileName = "WaveData", menuName = "2D Rogue-like/Wave Data")]
public class WaveData : SpawnData
{
    [Header("Wave Data")]

    [Tooltip("If there is less than this number of enemies alive, keep spawning enemies.")]
    [Min(0)] public int startingCount = 0;

    [Tooltip("How many enemies can this wave spawn at maximum?")]
    [Min(1)] public uint totalSpawns = uint.MaxValue;

    [System.Flags] public enum ExitCondition { waveDuration = 1, reachedTotalSpawns = 2 }
    [Tooltip("Set the things that can trigger the end of this wave")]
    public ExitCondition exitConditions = (ExitCondition)1;

    [Tooltip("All enemies must be dead for the next wave to start.")]
    public bool mustKillAllEnemies = false;

    [HideInInspector] public uint spawnedCount;

    public override GameObject[] GetSpawns(int totalEnemies = 0)
    {
        int count = Random.Range(spawnCount.x, spawnCount.y);

        if (totalEnemies + count < startingCount)
        {
            count = startingCount - totalEnemies;
        }

        GameObject[] result = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = possibleSpawnablePrefabs[Random.Range(0, possibleSpawnablePrefabs.Length)];
        }

        return result;
    }
}
