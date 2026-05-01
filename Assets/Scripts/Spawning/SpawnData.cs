using UnityEditor.EditorTools;
using UnityEngine;

public abstract class SpawnData : ScriptableObject
{
    [Tooltip("A list of all possible GameObjects that can be spawned.")]
    public GameObject[] possibleSpawnablePrefabs = new GameObject[1];

    [Tooltip("Time between each spawn (in seconds). Will take a random number between X and Y.")]
    public Vector2 spawnInterval = new Vector2(2, 3);

    [Tooltip("How many enemies are spawned per interval?")]
    public Vector2Int spawnCount = new Vector2Int(1, 1);

    [Tooltip("How long (in seconds) this will spawn enemies for.")]
    [Min(0.1f)] public float duration = 60;

    public virtual GameObject[] GetSpawns(int totalEnemies = 0)
    {
        int count = Random.Range(spawnCount.x, spawnCount.y);

        GameObject[] result = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = possibleSpawnablePrefabs[Random.Range(0, possibleSpawnablePrefabs.Length)];
        }

        return result;
    }

    public virtual float GetSpawnInterval()
    {
        return Random.Range(spawnInterval.x, spawnInterval.y);
    }
}
