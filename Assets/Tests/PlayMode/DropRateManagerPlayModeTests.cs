using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class DropRateManagerPlayModeTests
{
    [UnityTearDown]
    public IEnumerator TearDown()
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.Destroy(go);
        }

        yield return null;
    }

    [UnityTest]
    public IEnumerator OnDestroy_WhenDropRateIs100_ShouldInstantiateDrop()
    {
        GameObject managerObject = new GameObject("DropManager");
        DropRateManager manager = managerObject.AddComponent<DropRateManager>();
        manager.active = true;

        GameObject dropPrefab = new GameObject("DropPrefab");
        dropPrefab.AddComponent<TestDropMarker>();

        manager.drops = new List<DropRateManager.Drops>
        {
            new DropRateManager.Drops
            {
                dropName = "GuaranteedDrop",
                dropPrefab = dropPrefab,
                dropRate = 100f
            }
        };

        int before = Object.FindObjectsByType<TestDropMarker>(FindObjectsSortMode.None).Length;

        Object.Destroy(managerObject);
        yield return null;

        int after = Object.FindObjectsByType<TestDropMarker>(FindObjectsSortMode.None).Length;

        Assert.Greater(after, before);
    }

    [UnityTest]
    public IEnumerator OnDestroy_WhenAllDropRatesAreZero_ShouldNotInstantiateDrop()
    {
        GameObject managerObject = new GameObject("DropManager");
        DropRateManager manager = managerObject.AddComponent<DropRateManager>();
        manager.active = true;

        GameObject dropPrefab = new GameObject("DropPrefab");
        dropPrefab.AddComponent<TestDropMarker>();

        manager.drops = new List<DropRateManager.Drops>
        {
            new DropRateManager.Drops
            {
                dropName = "ImpossibleDrop",
                dropPrefab = dropPrefab,
                dropRate = 0f
            }
        };

        int before = Object.FindObjectsByType<TestDropMarker>(FindObjectsSortMode.None).Length;

        Object.Destroy(managerObject);
        yield return null;

        int after = Object.FindObjectsByType<TestDropMarker>(FindObjectsSortMode.None).Length;

        Assert.AreEqual(before, after);
    }

    [UnityTest]
    public IEnumerator OnDestroy_WhenExperienceGemCapReached_ShouldNotInstantiateExperienceDrop()
    {
        for (int i = 0; i < Pickup.MaxExperienceGemsOnMap; i++)
        {
            Pickup existingGem = new GameObject("Existing Experience Gem").AddComponent<Pickup>();
            existingGem.experience = 1;
        }

        yield return null;

        GameObject managerObject = new GameObject("DropManager");
        DropRateManager manager = managerObject.AddComponent<DropRateManager>();
        manager.active = true;

        GameObject dropPrefab = new GameObject("ExperienceDropPrefab");
        Pickup pickup = dropPrefab.AddComponent<Pickup>();
        pickup.experience = 5;
        dropPrefab.AddComponent<TestDropMarker>();
        dropPrefab.SetActive(false);

        manager.drops = new List<DropRateManager.Drops>
        {
            new DropRateManager.Drops
            {
                dropName = "ExperienceDrop",
                dropPrefab = dropPrefab,
                dropRate = 100f
            }
        };

        int before = Object.FindObjectsByType<TestDropMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;

        Object.Destroy(managerObject);
        yield return null;

        int after = Object.FindObjectsByType<TestDropMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;

        Assert.AreEqual(Pickup.MaxExperienceGemsOnMap, Pickup.ActiveExperienceGemCount);
        Assert.AreEqual(before, after);
    }
}
