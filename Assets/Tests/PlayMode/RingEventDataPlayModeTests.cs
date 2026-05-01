using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class RingEventDataPlayModeTests
{
    private class TestRingMarker : MonoBehaviour
    {
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.Destroy(obj);
        }

        yield return null;

        foreach (var data in Resources.FindObjectsOfTypeAll<RingEventData>())
        {
            Object.DestroyImmediate(data, true);
        }
    }

    [UnityTest]
    public IEnumerator Active_WhenPlayerExists_ShouldSpawnObjectsInCircle()
    {
        RingEventData data = ScriptableObject.CreateInstance<RingEventData>();
        data.spawnCount = new Vector2Int(4, 5);
        data.spawnRadius = 3f;
        data.scale = Vector2.one;
        data.lifespan = 0f;

        GameObject prefab = new GameObject("RingPrefab");
        prefab.AddComponent<TestRingMarker>();

        data.possibleSpawnablePrefabs = new GameObject[] { prefab };

        GameObject playerObject = new GameObject("Player");
        playerObject.SetActive(false);

        PlayerStats player = playerObject.AddComponent<PlayerStats>();
        player.enabled = false;
        playerObject.transform.position = Vector3.zero;

        int before = Object.FindObjectsByType<TestRingMarker>(FindObjectsSortMode.None).Length;

        bool result = data.Active(player);

        yield return null;

        TestRingMarker[] all = Object.FindObjectsByType<TestRingMarker>(FindObjectsSortMode.None);
        int spawnedCount = all.Length - before;

        Assert.AreEqual(4, spawnedCount);
        Assert.IsFalse(result);
    }

    [UnityTest]
    public IEnumerator Active_WhenPlayerIsNull_ShouldNotSpawn()
    {
        RingEventData data = ScriptableObject.CreateInstance<RingEventData>();
        data.spawnCount = new Vector2Int(4, 5);

        GameObject prefab = new GameObject("RingPrefab");
        prefab.AddComponent<TestRingMarker>();

        data.possibleSpawnablePrefabs = new GameObject[] { prefab };

        int before = Object.FindObjectsByType<TestRingMarker>(FindObjectsSortMode.None).Length;

        bool result = data.Active(null);

        yield return null;

        int after = Object.FindObjectsByType<TestRingMarker>(FindObjectsSortMode.None).Length;

        Assert.AreEqual(before, after);
        Assert.IsFalse(result);
    }

    [UnityTest]
    public IEnumerator Active_WhenMultiplePrefabs_ShouldStillSpawnCorrectCount()
    {
        RingEventData data = ScriptableObject.CreateInstance<RingEventData>();
        data.spawnCount = new Vector2Int(6, 7);
        data.spawnRadius = 2f;
        data.scale = Vector2.one;
        data.lifespan = 0f;

        GameObject prefab1 = new GameObject("A");
        prefab1.AddComponent<TestRingMarker>();

        GameObject prefab2 = new GameObject("B");
        prefab2.AddComponent<TestRingMarker>();

        data.possibleSpawnablePrefabs = new GameObject[] { prefab1, prefab2 };

        GameObject playerObject = new GameObject("Player");
        playerObject.SetActive(false);

        PlayerStats player = playerObject.AddComponent<PlayerStats>();
        player.enabled = false;
        playerObject.transform.position = Vector3.zero;

        int before = Object.FindObjectsByType<TestRingMarker>(FindObjectsSortMode.None).Length;

        data.Active(player);

        yield return null;

        int after = Object.FindObjectsByType<TestRingMarker>(FindObjectsSortMode.None).Length;

        Assert.AreEqual(6, after - before);
    }
}