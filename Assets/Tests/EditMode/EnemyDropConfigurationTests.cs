using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class EnemyDropConfigurationTests
{
    const string BlueGemPath = "Assets/Prefabs/Pickups/Blue Gem.prefab";
    const string GreenGemPath = "Assets/Prefabs/Pickups/Green Gem.prefab";
    const string RedGemPath = "Assets/Prefabs/Pickups/Red Gem.prefab";

    static readonly object[] ExpectedGemDrops =
    {
        new object[] { "Assets/Prefabs/Enemies/Bat.prefab", 3f, null },
        new object[] { "Assets/Prefabs/Enemies/Zombie.prefab", 5f, 0.5f },
        new object[] { "Assets/Prefabs/Enemies/Skeleton.prefab", 6f, 1f },
        new object[] { "Assets/Prefabs/Enemies/Red Bat.prefab", 10f, 2f },
        new object[] { "Assets/Prefabs/Enemies/Plant.prefab", 12f, 3f },
        new object[] { "Assets/Prefabs/Enemies/Reaper.prefab", null, 100f }
    };

    [TestCaseSource(nameof(ExpectedGemDrops))]
    public void EnemyPrefabs_ShouldKeepBlueGemAtFiftyAndConfigureAdvancedGemDrops(string enemyPath, float? greenRate, float? redRate)
    {
        DropRateManager manager = LoadDropRateManager(enemyPath);
        GameObject blueGem = AssetDatabase.LoadAssetAtPath<GameObject>(BlueGemPath);
        GameObject greenGem = AssetDatabase.LoadAssetAtPath<GameObject>(GreenGemPath);
        GameObject redGem = AssetDatabase.LoadAssetAtPath<GameObject>(RedGemPath);

        AssertDrop(manager, "Blue gem", blueGem, 50f);

        if (greenRate.HasValue)
            AssertDrop(manager, "Green gem", greenGem, greenRate.Value);
        else
            AssertNoDrop(manager, "Green gem");

        if (redRate.HasValue)
            AssertDrop(manager, "Red gem", redGem, redRate.Value);
        else
            AssertNoDrop(manager, "Red gem");
    }

    DropRateManager LoadDropRateManager(string enemyPath)
    {
        GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(enemyPath);
        Assert.NotNull(enemyPrefab, enemyPath);

        DropRateManager manager = enemyPrefab.GetComponent<DropRateManager>();
        Assert.NotNull(manager, enemyPath);
        Assert.NotNull(manager.drops, enemyPath);
        return manager;
    }

    void AssertDrop(DropRateManager manager, string dropName, GameObject expectedPrefab, float expectedRate)
    {
        DropRateManager.Drops drop = FindDrop(manager.drops, dropName);

        Assert.NotNull(drop, dropName);
        Assert.AreSame(expectedPrefab, drop.dropPrefab);
        Assert.AreEqual(expectedRate, drop.dropRate, 0.001f);
    }

    void AssertNoDrop(DropRateManager manager, string dropName)
    {
        Assert.IsNull(FindDrop(manager.drops, dropName), dropName);
    }

    DropRateManager.Drops FindDrop(List<DropRateManager.Drops> drops, string dropName)
    {
        foreach (DropRateManager.Drops drop in drops)
        {
            if (drop.dropName == dropName) return drop;
        }

        return null;
    }
}
