using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class DropRateManagerPlayModeTests
{
    [UnityTest]
    public IEnumerator OnDestroy_WhenDropRateIs100_ShouldInstantiateDrop()
    {
        GameObject managerObject = new GameObject("DropManager");
        DropRateManager manager = managerObject.AddComponent<DropRateManager>();

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
        yield return null;

        int after = Object.FindObjectsByType<TestDropMarker>(FindObjectsSortMode.None).Length;

        Assert.Greater(after, before);

        Object.Destroy(dropPrefab);
    }

    [UnityTest]
    public IEnumerator OnDestroy_WhenAllDropRatesAreZero_ShouldNotInstantiateDrop()
    {
        GameObject managerObject = new GameObject("DropManager");
        DropRateManager manager = managerObject.AddComponent<DropRateManager>();

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
        yield return null;

        int after = Object.FindObjectsByType<TestDropMarker>(FindObjectsSortMode.None).Length;

        Assert.AreEqual(before, after);

        Object.Destroy(dropPrefab);
    }
}