using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventoryRemainingWeaponTests
{
    private object InvokePrivate(object target, string methodName, params object[] args)
    {
        return target.GetType()
            .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(target, args);
    }

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
            description = name + "_Description"
        };
        data.linearGrowth = new Weapon.Stats[]
        {
            new Weapon.Stats
            {
                name = name + "_L2",
                description = name + "_L2_Description"
            }
        };
        data.randomGrowth = new Weapon.Stats[0];
        return data;
    }

    private PlayerInventory.Slot CreateSlot(Item item)
    {
        return new PlayerInventory.Slot
        {
            item = item,
        };
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(obj);
        }

        foreach (var weaponData in Resources.FindObjectsOfTypeAll<WeaponData>())
        {
            Object.DestroyImmediate(weaponData, true);
        }
    }

    [Test]
    public void GetSlotsLeft_WhenNoSlots_ShouldReturnZero()
    {
        PlayerInventory inventory = CreateInventory();

        int result = (int)InvokePrivate(inventory, "GetSlotsLeft", inventory.weaponSlots);

        Assert.AreEqual(0, result);
    }

    [Test]
    public void GetSlotsLeft_WhenOneEmptySlot_ShouldReturnOne()
    {
        PlayerInventory inventory = CreateInventory();

        inventory.weaponSlots.Add(CreateSlot(null));

        int result = (int)InvokePrivate(inventory, "GetSlotsLeft", inventory.weaponSlots);

        Assert.AreEqual(1, result);
    }

    [Test]
    public void CanBeOfferedAsUpgrade_WhenNewWeaponAndSlotAvailable_ShouldReturnTrue()
    {
        PlayerInventory inventory = CreateInventory();
        WeaponData weaponData = CreateWeaponData("Axe");

        bool result = (bool)InvokePrivate(
            inventory,
            "CanBeOfferedAsUpgrade",
            weaponData,
            1,
            0
        );

        Assert.IsTrue(result);
    }

    [Test]
    public void CanBeOfferedAsUpgrade_WhenNewWeaponAndNoSlotAvailable_ShouldReturnFalse()
    {
        PlayerInventory inventory = CreateInventory();
        WeaponData weaponData = CreateWeaponData("Axe");

        bool result = (bool)InvokePrivate(
            inventory,
            "CanBeOfferedAsUpgrade",
            weaponData,
            0,
            0
        );

        Assert.IsFalse(result);
    }

    [Test]
    public void CanBeOfferedAsUpgrade_WhenExistingWeaponBelowMax_ShouldReturnTrue()
    {
        PlayerInventory inventory = CreateInventory();
        WeaponData weaponData = CreateWeaponData("Knife", 5);

        GameObject weaponObject = new GameObject("Knife");
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();
        weapon.data = weaponData;
        weapon.currentLevel = 2;

        inventory.weaponSlots.Add(CreateSlot(weapon));

        bool result = (bool)InvokePrivate(
            inventory,
            "CanBeOfferedAsUpgrade",
            weaponData,
            0,
            0
        );

        Assert.IsTrue(result);
    }

    [Test]
    public void CanBeOfferedAsUpgrade_WhenExistingWeaponAtMax_ShouldReturnFalse()
    {
        PlayerInventory inventory = CreateInventory();
        WeaponData weaponData = CreateWeaponData("Knife", 2);

        GameObject weaponObject = new GameObject("Knife");
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();
        weapon.data = weaponData;
        weapon.currentLevel = 2;

        inventory.weaponSlots.Add(CreateSlot(weapon));

        bool result = (bool)InvokePrivate(
            inventory,
            "CanBeOfferedAsUpgrade",
            weaponData,
            0,
            0
        );

        Assert.IsFalse(result);
    }

    [Test]
    public void GetAvailableUpgrades_WhenAvailableWeaponAndEmptySlot_ShouldIncludeWeapon()
    {
        PlayerInventory inventory = CreateInventory();
        WeaponData weaponData = CreateWeaponData("Cross");

        inventory.availableWeapons.Add(weaponData);
        inventory.weaponSlots.Add(CreateSlot(null));

        List<ItemData> result = (List<ItemData>)InvokePrivate(inventory, "GetAvailableUpgrades");

        Assert.Contains(weaponData, result);
    }

    [Test]
    public void GetAvailableUpgrades_WhenAvailableWeaponButNoEmptySlot_ShouldNotIncludeWeapon()
    {
        PlayerInventory inventory = CreateInventory();
        WeaponData weaponData = CreateWeaponData("Cross");

        inventory.availableWeapons.Add(weaponData);

        List<ItemData> result = (List<ItemData>)InvokePrivate(inventory, "GetAvailableUpgrades");

        Assert.IsFalse(result.Contains(weaponData));
    }
}