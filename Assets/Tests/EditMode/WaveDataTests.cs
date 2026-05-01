using NUnit.Framework;
using UnityEngine;

public class WaveDataTests
{
    [TearDown]
    public void TearDown()
    {
        foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(obj);
        }

        foreach (var data in Resources.FindObjectsOfTypeAll<WaveData>())
        {
            Object.DestroyImmediate(data, true);
        }
    }

    [Test]
    public void GetSpawns_WhenSpawnCountIsFixed_ShouldReturnCorrectCount()
    {
        WaveData data = ScriptableObject.CreateInstance<WaveData>();

        GameObject prefab = new GameObject("EnemyPrefab");

        data.possibleSpawnablePrefabs = new GameObject[]
        {
            prefab
        };

        data.spawnCount = new Vector2Int(3, 4);

        GameObject[] result = data.GetSpawns();

        Assert.AreEqual(3, result.Length);
    }

    [Test]
    public void GetSpawns_WhenMultiplePrefabsExist_ShouldReturnOnlyAllowedPrefabs()
    {
        WaveData data = ScriptableObject.CreateInstance<WaveData>();

        GameObject prefabA = new GameObject("EnemyA");
        GameObject prefabB = new GameObject("EnemyB");

        data.possibleSpawnablePrefabs = new GameObject[]
        {
            prefabA,
            prefabB
        };

        data.spawnCount = new Vector2Int(5, 6);

        GameObject[] result = data.GetSpawns();

        Assert.AreEqual(5, result.Length);

        foreach (GameObject spawn in result)
        {
            Assert.IsTrue(spawn == prefabA || spawn == prefabB);
        }
    }

    [Test]
    public void GetSpawnInterval_WhenIntervalIsFixed_ShouldReturnValue()
    {
        WaveData data = ScriptableObject.CreateInstance<WaveData>();

        data.spawnInterval = new Vector2(2.5f, 2.5f);

        float result = data.GetSpawnInterval();

        Assert.AreEqual(2.5f, result);
    }

    [Test]
    public void GetSpawnInterval_WhenIntervalHasRange_ShouldReturnValueInsideRange()
    {
        WaveData data = ScriptableObject.CreateInstance<WaveData>();

        data.spawnInterval = new Vector2(1f, 3f);

        float result = data.GetSpawnInterval();

        Assert.GreaterOrEqual(result, 1f);
        Assert.LessOrEqual(result, 3f);
    }

    [Test]
    public void WaveData_ShouldStoreWaveConfiguration()
    {
        WaveData data = ScriptableObject.CreateInstance<WaveData>();

        data.totalSpawns = 20;
        data.duration = 30f;
        data.exitConditions = WaveData.ExitCondition.waveDuration;
        data.mustKillAllEnemies = true;

        Assert.AreEqual(20, data.totalSpawns);
        Assert.AreEqual(30f, data.duration);
        Assert.AreEqual(WaveData.ExitCondition.waveDuration, data.exitConditions);
        Assert.IsTrue(data.mustKillAllEnemies);
    }
}