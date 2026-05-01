using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MobEventDataPlayModeTests
{
    private class TestMobMarker : MonoBehaviour
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

        foreach (var data in Resources.FindObjectsOfTypeAll<MobEventData>())
        {
            Object.DestroyImmediate(data, true);
        }
    }

    [UnityTest]
    public IEnumerator Active_WhenPlayerExists_ShouldInstantiateMobs()
    {
        MobEventData data = ScriptableObject.CreateInstance<MobEventData>();
        data.spawnCount = new Vector2Int(2, 2);
        data.spawnInterval = new Vector2(1f, 1f);
        data.possibleAngles = 360f;
        data.spawnRadius = 0f;
        data.spawnDistance = 5f;

        GameObject prefab = new GameObject("MobPrefab");
        prefab.AddComponent<TestMobMarker>();

        data.possibleSpawnablePrefabs = new GameObject[]
        {
            prefab
        };

        GameObject playerObject = new GameObject("Player");
        playerObject.SetActive(false);

        PlayerStats player = playerObject.AddComponent<PlayerStats>();
        player.enabled = false;

        playerObject.transform.position = Vector3.zero;

        int before = Object.FindObjectsByType<TestMobMarker>(FindObjectsSortMode.None).Length;

        bool result = data.Active(player);

        yield return null;

        int after = Object.FindObjectsByType<TestMobMarker>(FindObjectsSortMode.None).Length;

        Assert.IsFalse(result);
        Assert.AreEqual(before + 2, after);
    }

    [UnityTest]
    public IEnumerator Active_WhenPlayerIsNull_ShouldNotInstantiateMobs()
    {
        MobEventData data = ScriptableObject.CreateInstance<MobEventData>();
        data.spawnCount = new Vector2Int(2, 2);

        GameObject prefab = new GameObject("MobPrefab");
        prefab.AddComponent<TestMobMarker>();

        data.possibleSpawnablePrefabs = new GameObject[]
        {
            prefab
        };

        int before = Object.FindObjectsByType<TestMobMarker>(FindObjectsSortMode.None).Length;

        bool result = data.Active(null);

        yield return null;

        int after = Object.FindObjectsByType<TestMobMarker>(FindObjectsSortMode.None).Length;

        Assert.IsFalse(result);
        Assert.AreEqual(before, after);
    }
}