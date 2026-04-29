using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ItemTests
{
    private class TestItem : Item
    {
        public void SetInventory(PlayerInventory playerInventory)
        {
            inventory = playerInventory;
        }
    }

    private class TestWeapon : Weapon
    {
    }

    private WeaponData CreateWeaponData(string itemName = "WeaponData")
    {
        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
        data.name = itemName;
        data.maxLevel = 5;
        data.baseStats = new Weapon.Stats
        {
            name = itemName
        };
        data.linearGrowth = new Weapon.Stats[0];
        data.randomGrowth = new Weapon.Stats[0];
        return data;
    }

    private PlayerInventory.Slot CreateWeaponSlot(Item item)
    {
        return new PlayerInventory.Slot
        {
            item = item,
        };
    }

    private ItemData.Evolution CreateEvolution(
        WeaponData catalystItem,
        int requiredLevel,
        WeaponData outcomeItem,
        int outcomeLevel,
        int evolutionLevel = 2)
    {
        return new ItemData.Evolution
        {
            name = "TestEvolution",
            condition = ItemData.Evolution.Condition.auto,
            consumes = ItemData.Evolution.Consumption.weapons,
            evolutionLevel = evolutionLevel,
            catalysts = new ItemData.Evolution.Config[]
            {
                new ItemData.Evolution.Config
                {
                    itemType = catalystItem,
                    level = requiredLevel
                }
            },
            outcome = new ItemData.Evolution.Config
            {
                itemType = outcomeItem,
                level = outcomeLevel
            }
        };
    }

    [Test]
    public void CanEvolve_WhenAllConditionsAreMet_ShouldReturnTrue()
    {
        GameObject inventoryObject = new GameObject("Inventory");
        PlayerInventory inventory = inventoryObject.AddComponent<PlayerInventory>();
        inventory.weaponSlots = new List<PlayerInventory.Slot>();
        inventory.passiveSlots = new List<PlayerInventory.Slot>();

        GameObject itemObject = new GameObject("Item");
        TestItem item = itemObject.AddComponent<TestItem>();
        item.currentLevel = 2;
        item.SetInventory(inventory);

        WeaponData catalystData = CreateWeaponData("Catalyst");
        WeaponData outcomeData = CreateWeaponData("Outcome");

        GameObject catalystObject = new GameObject("CatalystWeapon");
        TestWeapon catalystWeapon = catalystObject.AddComponent<TestWeapon>();
        catalystWeapon.data = catalystData;
        catalystWeapon.currentLevel = 1;

        inventory.weaponSlots.Add(CreateWeaponSlot(catalystWeapon));

        ItemData.Evolution evolution = CreateEvolution(catalystData, 1, outcomeData, 1);

        bool result = item.CanEvolve(evolution);

        Assert.IsTrue(result);

        Object.DestroyImmediate(inventoryObject);
        Object.DestroyImmediate(itemObject);
        Object.DestroyImmediate(catalystObject);
        Object.DestroyImmediate(catalystData);
        Object.DestroyImmediate(outcomeData);
    }

    [Test]
    public void CanEvolve_WhenCatalystIsMissing_ShouldReturnFalse()
    {
        GameObject inventoryObject = new GameObject("Inventory");
        PlayerInventory inventory = inventoryObject.AddComponent<PlayerInventory>();
        inventory.weaponSlots = new List<PlayerInventory.Slot>();
        inventory.passiveSlots = new List<PlayerInventory.Slot>();

        GameObject itemObject = new GameObject("Item");
        TestItem item = itemObject.AddComponent<TestItem>();
        item.currentLevel = 2;
        item.SetInventory(inventory);

        WeaponData catalystData = CreateWeaponData("Catalyst");
        WeaponData outcomeData = CreateWeaponData("Outcome");

        ItemData.Evolution evolution = CreateEvolution(catalystData, 1, outcomeData, 1);

        bool result = item.CanEvolve(evolution);

        Assert.IsFalse(result);

        Object.DestroyImmediate(inventoryObject);
        Object.DestroyImmediate(itemObject);
        Object.DestroyImmediate(catalystData);
        Object.DestroyImmediate(outcomeData);
    }

    [Test]
    public void CanEvolve_WhenCurrentLevelIsTooLow_ShouldReturnFalse()
    {
        GameObject inventoryObject = new GameObject("Inventory");
        PlayerInventory inventory = inventoryObject.AddComponent<PlayerInventory>();
        inventory.weaponSlots = new List<PlayerInventory.Slot>();
        inventory.passiveSlots = new List<PlayerInventory.Slot>();

        GameObject itemObject = new GameObject("Item");
        TestItem item = itemObject.AddComponent<TestItem>();
        item.currentLevel = 1;
        item.SetInventory(inventory);

        WeaponData catalystData = CreateWeaponData("Catalyst");
        WeaponData outcomeData = CreateWeaponData("Outcome");

        GameObject catalystObject = new GameObject("CatalystWeapon");
        TestWeapon catalystWeapon = catalystObject.AddComponent<TestWeapon>();
        catalystWeapon.data = catalystData;
        catalystWeapon.currentLevel = 1;

        inventory.weaponSlots.Add(CreateWeaponSlot(catalystWeapon));

        ItemData.Evolution evolution = CreateEvolution(catalystData, 1, outcomeData, 1, 2);

        bool result = item.CanEvolve(evolution, 0);

        Assert.IsFalse(result);

        Object.DestroyImmediate(inventoryObject);
        Object.DestroyImmediate(itemObject);
        Object.DestroyImmediate(catalystObject);
        Object.DestroyImmediate(catalystData);
        Object.DestroyImmediate(outcomeData);
    }

    [Test]
    public void CanLevelUp_WhenCurrentLevelIsBelowMaxLevel_ShouldReturnTrue()
    {
        GameObject itemObject = new GameObject("Item");
        TestItem item = itemObject.AddComponent<TestItem>();
        item.currentLevel = 1;
        item.maxLevel = 3;

        bool result = item.CanLevelUp();

        Assert.IsTrue(result);

        Object.DestroyImmediate(itemObject);
    }

    [Test]
    public void CanLevelUp_WhenCurrentLevelEqualsMaxLevel_ShouldReturnFalse()
    {
        GameObject itemObject = new GameObject("Item");
        TestItem item = itemObject.AddComponent<TestItem>();
        item.currentLevel = 3;
        item.maxLevel = 3;

        bool result = item.CanLevelUp();

        Assert.IsFalse(result);

        Object.DestroyImmediate(itemObject);
    }
}