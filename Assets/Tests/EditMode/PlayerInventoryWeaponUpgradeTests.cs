using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class PlayerInventoryWeaponUpgradeTests
{
    private class TestWeapon : Weapon
    {
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

    private WeaponData CreateWeaponData(string name, int maxLevel = 5)
    {
        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
        data.name = name;
        data.maxLevel = maxLevel;
        data.baseStats = new Weapon.Stats
        {
            name = name,
            description = name + "_Base"
        };
        data.linearGrowth = new Weapon.Stats[]
        {
            new Weapon.Stats
            {
                name = name + "_L2",
                description = name + "_Desc_L2"
            },
            new Weapon.Stats
            {
                name = name + "_L3",
                description = name + "_Desc_L3"
            }
        };
        data.randomGrowth = new Weapon.Stats[0];
        return data;
    }

    private PlayerInventory.Slot CreateSlot(Item item)
    {
        GameObject imageObject = new GameObject("Image");
        Image image = imageObject.AddComponent<Image>();

        return new PlayerInventory.Slot
        {
            item = item,
            image = image
        };
    }

    private void SetPrivateField(object target, string fieldName, object value)
    {
        target.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(target, value);
    }

    private void SetCurrentStats(Weapon weapon, Weapon.Stats stats)
    {
        typeof(Weapon)
            .GetField("currentStats", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(weapon, stats);
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(obj);
        }

        foreach (var data in Resources.FindObjectsOfTypeAll<WeaponData>())
        {
            Object.DestroyImmediate(data, true);
        }
    }

    [Test]
    public void LevelUp_WhenWeaponIsValid_ShouldIncreaseWeaponLevel()
    {
        PlayerInventory inventory = CreateInventory();

        WeaponData weaponData = CreateWeaponData("MagicWand", 5);

        GameObject weaponObject = new GameObject("Weapon");
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();
        weapon.data = weaponData;
        weapon.maxLevel = weaponData.maxLevel;
        weapon.currentLevel = 1;

        SetCurrentStats(weapon, weaponData.baseStats);
        SetPrivateField(weapon, "evolutionData", new ItemData.Evolution[0]);

        inventory.weaponSlots.Add(CreateSlot(weapon));

        bool result = inventory.LevelUp(weapon);

        Assert.IsTrue(result);
        Assert.AreEqual(2, weapon.currentLevel);
    }

    [Test]
    public void LevelUp_WhenWeaponIsAtMaxLevel_ShouldReturnFalse()
    {
        PlayerInventory inventory = CreateInventory();

        WeaponData weaponData = CreateWeaponData("Whip", 2);

        GameObject weaponObject = new GameObject("Weapon");
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();
        weapon.data = weaponData;
        weapon.maxLevel = weaponData.maxLevel;
        weapon.currentLevel = 2;

        SetPrivateField(weapon, "evolutionData", new ItemData.Evolution[0]);

        inventory.weaponSlots.Add(CreateSlot(weapon));

        LogAssert.Expect(LogType.Warning, "Cannot level up Weapon to Level 2, max level of 2 already reached.");
        LogAssert.Expect(LogType.Warning, "Failed to level up Weapon");

        bool result = inventory.LevelUp(weapon);

        Assert.IsFalse(result);
        Assert.AreEqual(2, weapon.currentLevel);
    }

    [Test]
    public void LevelUp_WhenItemIsNull_ShouldReturnFalse()
    {
        PlayerInventory inventory = CreateInventory();

        bool result = inventory.LevelUp(null);

        Assert.IsFalse(result);
    }
}