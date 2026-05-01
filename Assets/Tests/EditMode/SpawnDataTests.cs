using NUnit.Framework;
using UnityEngine;

public class SpawnDataTests
{
    private class TestSpawnData : SpawnData
    {
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(obj);
        }

        foreach (var data in Resources.FindObjectsOfTypeAll<TestSpawnData>())
        {
            Object.DestroyImmediate(data, true);
        }
    }

    [Test]
    public void GetSpawns_WhenSpawnCountIsFixed_ShouldReturnCorrectNumberOfPrefabs()
    {
        TestSpawnData data = ScriptableObject.CreateInstance<TestSpawnData>();

        GameObject prefab = new GameObject("EnemyPrefab");

        data.possibleSpawnablePrefabs = new GameObject[]
        {
            prefab
        };

        data.spawnCount = new Vector2Int(3, 3);

        GameObject[] result = data.GetSpawns();

        Assert.AreEqual(3, result.Length);
        Assert.AreSame(prefab, result[0]);
        Assert.AreSame(prefab, result[1]);
        Assert.AreSame(prefab, result[2]);
    }

    [Test]
    public void GetSpawns_WhenMultiplePrefabsExist_ShouldOnlyReturnPrefabsFromPossibleList()
    {
        TestSpawnData data = ScriptableObject.CreateInstance<TestSpawnData>();

        GameObject prefabA = new GameObject("EnemyA");
        GameObject prefabB = new GameObject("EnemyB");

        data.possibleSpawnablePrefabs = new GameObject[]
        {
            prefabA,
            prefabB
        };

        data.spawnCount = new Vector2Int(5, 5);

        GameObject[] result = data.GetSpawns();

        Assert.AreEqual(5, result.Length);

        foreach (GameObject spawn in result)
        {
            Assert.IsTrue(spawn == prefabA || spawn == prefabB);
        }
    }

    [Test]
    public void GetSpawnInterval_WhenIntervalIsFixed_ShouldReturnThatValue()
    {
        TestSpawnData data = ScriptableObject.CreateInstance<TestSpawnData>();

        data.spawnInterval = new Vector2(2.5f, 2.5f);

        float result = data.GetSpawnInterval();

        Assert.AreEqual(2.5f, result);
    }

    [Test]
    public void GetSpawnInterval_WhenIntervalHasRange_ShouldReturnValueInsideRange()
    {
        TestSpawnData data = ScriptableObject.CreateInstance<TestSpawnData>();

        data.spawnInterval = new Vector2(2f, 5f);

        float result = data.GetSpawnInterval();

        Assert.GreaterOrEqual(result, 2f);
        Assert.LessOrEqual(result, 5f);
    }
}