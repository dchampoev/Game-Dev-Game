using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class PlayerInventoryPassiveUpgradeTests
{
    private class TestPassive : Passive
    {
    }

    private void SetPrivateField(object target, string fieldName, object value)
    {
        target.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(target, value);
    }

    private PlayerInventory CreateInventory()
    {
        GameObject go = new GameObject("Inventory");
        PlayerInventory inventory = go.AddComponent<PlayerInventory>();
        inventory.weaponSlots = new List<PlayerInventory.Slot>();
        inventory.passiveSlots = new List<PlayerInventory.Slot>();
        inventory.availableWeapons = new List<WeaponData>();
        inventory.availablePassives = new List<PassiveData>();
        return inventory;
    }

    private PassiveData CreatePassiveData(string name, int maxLevel = 5)
    {
        PassiveData data = ScriptableObject.CreateInstance<PassiveData>();
        data.name = name;
        data.maxLevel = maxLevel;
        data.baseStats = new Passive.Modifier
        {
            name = name,
            description = name + "_Base",
            boosts = new CharacterData.Stats
            {
                maxHealth = 1f,
                might = 1f
            }
        };
        data.growth = new Passive.Modifier[]
        {
            new Passive.Modifier
            {
                name = name + "_L2",
                description = name + "_Desc_L2",
                boosts = new CharacterData.Stats
                {
                    might = 2f
                }
            },
            new Passive.Modifier
            {
                name = name + "_L3",
                description = name + "_Desc_L3",
                boosts = new CharacterData.Stats
                {
                    might = 3f
                }
            }
        };
        return data;
    }

    private PlayerInventory.Slot CreateSlot(Item item)
    {
        return new PlayerInventory.Slot
        {
            item = item,
        };
    }

    private PlayerStats CreatePlayerStats(PlayerInventory inventory)
    {
        GameObject playerObject = new GameObject("Player");
        playerObject.SetActive(false);

        PlayerStats playerStats = playerObject.AddComponent<PlayerStats>();
        playerStats.enabled = false;

        GameObject collectorObject = new GameObject("Collector");
        collectorObject.transform.SetParent(playerObject.transform);
        collectorObject.AddComponent<CircleCollider2D>();
        PlayerCollector collector = collectorObject.AddComponent<PlayerCollector>();
        collector.enabled = false;

        CharacterData.Stats stats = new CharacterData.Stats
        {
            maxHealth = 20f,
            recovery = 1f,
            armor = 0f,
            moveSpeed = 5f,
            might = 1f,
            area = 1f,
            speed = 1f,
            duration = 1f,
            amount = 0,
            cooldown = 1f,
            luck = 1f,
            growth = 1f,
            greed = 1f,
            curse = 0f,
            magnet = 1f,
            revival = 0
        };

        CharacterData characterData = ScriptableObject.CreateInstance<CharacterData>();

        playerStats.baseStats = stats;
        playerStats.Stats = stats;
        playerStats.CurrentHealth = stats.maxHealth;

        SetPrivateField(playerStats, "inventory", inventory);
        SetPrivateField(playerStats, "collector", collector);
        SetPrivateField(playerStats, "characterData", characterData);
        SetPrivateField(playerStats, "health", stats.maxHealth);

        return playerStats;
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(obj);
        }

        foreach (var passiveData in Resources.FindObjectsOfTypeAll<PassiveData>())
        {
            Object.DestroyImmediate(passiveData, true);
        }

        foreach (var characterData in Resources.FindObjectsOfTypeAll<CharacterData>())
        {
            Object.DestroyImmediate(characterData, true);
        }
    }

    [Test]
    public void LevelUp_WhenPassiveIsValid_ShouldIncreasePassiveLevel()
    {
        PlayerInventory inventory = CreateInventory();
        PlayerStats playerStats = CreatePlayerStats(inventory);
        SetPrivateField(inventory, "player", playerStats);

        PassiveData passiveData = CreatePassiveData("HollowHeart", 5);

        GameObject passiveObject = new GameObject("Passive");
        TestPassive passive = passiveObject.AddComponent<TestPassive>();
        passive.data = passiveData;
        passive.maxLevel = passiveData.maxLevel;
        passive.currentLevel = 1;

        inventory.passiveSlots.Add(CreateSlot(passive));

        bool result = inventory.LevelUp(passive);

        Assert.IsTrue(result);
        Assert.AreEqual(2, passive.currentLevel);
    }

    [Test]
    public void LevelUp_WhenPassiveIsValid_ShouldRecalculatePlayerStats()
    {
        PlayerInventory inventory = CreateInventory();
        PlayerStats playerStats = CreatePlayerStats(inventory);
        SetPrivateField(inventory, "player", playerStats);

        PassiveData passiveData = CreatePassiveData("Spinach", 5);

        GameObject passiveObject = new GameObject("Passive");
        TestPassive passive = passiveObject.AddComponent<TestPassive>();
        passive.data = passiveData;
        passive.maxLevel = passiveData.maxLevel;
        passive.currentLevel = 1;

        inventory.passiveSlots.Add(CreateSlot(passive));

        bool result = inventory.LevelUp(passive);

        Assert.IsTrue(result);
        Assert.Greater(playerStats.Stats.might, playerStats.baseStats.might);
    }

    [Test]
    public void LevelUp_WhenItemIsNull_ShouldReturnFalse()
    {
        PlayerInventory inventory = CreateInventory();

        bool result = inventory.LevelUp(null);

        Assert.IsFalse(result);
    }

    [Test]
    public void LevelUp_WhenPassiveIsAtMaxLevel_ShouldReturnFalse()
    {
        PlayerInventory inventory = CreateInventory();
        PlayerStats playerStats = CreatePlayerStats(inventory);
        SetPrivateField(inventory, "player", playerStats);

        PassiveData passiveData = CreatePassiveData("Armor", 2);

        GameObject passiveObject = new GameObject("Passive");
        TestPassive passive = passiveObject.AddComponent<TestPassive>();
        passive.data = passiveData;
        passive.maxLevel = passiveData.maxLevel;
        passive.currentLevel = 2;

        inventory.passiveSlots.Add(CreateSlot(passive));

        LogAssert.Expect(LogType.Warning, "Cannot level up Passive to Level 2, max level of 2 already reached.");
        LogAssert.Expect(LogType.Warning, "Failed to level up Passive");

        bool result = inventory.LevelUp(passive);

        Assert.IsFalse(result);
        Assert.AreEqual(2, passive.currentLevel);
    }
}