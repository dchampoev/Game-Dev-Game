using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class TreasureChestTests
{
    private class TestWeapon : Weapon
    {
        public int attemptCalls;
        public bool attemptResult;

        public override bool AttemptEvolution(ItemData.Evolution evolutionData, int levelUpAmount = 1)
        {
            attemptCalls++;
            return attemptResult;
        }
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

    private WeaponData CreateWeaponData(ItemData.Evolution[] evolutions = null)
    {
        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
        data.baseStats = new Weapon.Stats
        {
            name = "Weapon",
            description = "Weapon"
        };
        data.linearGrowth = new Weapon.Stats[0];
        data.randomGrowth = new Weapon.Stats[0];
        data.evolutionData = evolutions;
        return data;
    }

    private ItemData.Evolution CreateEvolution(ItemData.Evolution.Condition condition)
    {
        return new ItemData.Evolution
        {
            condition = condition,
            catalysts = new ItemData.Evolution.Config[0],
            outcome = new ItemData.Evolution.Config()
        };
    }

    [Test]
    public void OpenTreasureChest_WhenWeaponHasNoEvolutionData_ShouldSkipIt()
    {
        PlayerInventory inventory = CreateInventory();

        GameObject weaponObject = new GameObject("Weapon");
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();
        weapon.data = CreateWeaponData(null);

        inventory.weaponSlots.Add(new PlayerInventory.Slot
        {
            item = weapon
        });

        GameObject chestObject = new GameObject("Chest");
        TreasureChest chest = chestObject.AddComponent<TreasureChest>();

        chest.OpenTreasureChest(inventory, false);

        Assert.AreEqual(0, weapon.attemptCalls);
    }

    [Test]
    public void OpenTreasureChest_WhenEvolutionIsNotTreasureChest_ShouldNotAttemptEvolution()
    {
        PlayerInventory inventory = CreateInventory();

        GameObject weaponObject = new GameObject("Weapon");
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();
        weapon.data = CreateWeaponData(new[]
        {
            CreateEvolution(ItemData.Evolution.Condition.auto)
        });

        inventory.weaponSlots.Add(new PlayerInventory.Slot
        {
            item = weapon
        });

        GameObject chestObject = new GameObject("Chest");
        TreasureChest chest = chestObject.AddComponent<TreasureChest>();

        chest.OpenTreasureChest(inventory, false);

        Assert.AreEqual(0, weapon.attemptCalls);
    }

    [Test]
    public void OpenTreasureChest_WhenTreasureChestEvolutionExists_ShouldAttemptEvolutionWithLevelUpAmountZero()
    {
        PlayerInventory inventory = CreateInventory();

        GameObject weaponObject = new GameObject("Weapon");
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();
        weapon.attemptResult = false;
        weapon.data = CreateWeaponData(new[]
        {
            CreateEvolution(ItemData.Evolution.Condition.treasureChest)
        });

        inventory.weaponSlots.Add(new PlayerInventory.Slot
        {
            item = weapon
        });

        GameObject chestObject = new GameObject("Chest");
        TreasureChest chest = chestObject.AddComponent<TreasureChest>();

        chest.OpenTreasureChest(inventory, false);

        Assert.AreEqual(1, weapon.attemptCalls);
    }

    [Test]
    public void OpenTreasureChest_WhenFirstWeaponEvolutionSucceeds_ShouldReturnWithoutCheckingNextWeapon()
    {
        PlayerInventory inventory = CreateInventory();

        GameObject firstWeaponObject = new GameObject("FirstWeapon");
        TestWeapon firstWeapon = firstWeaponObject.AddComponent<TestWeapon>();
        firstWeapon.attemptResult = true;
        firstWeapon.data = CreateWeaponData(new[]
        {
            CreateEvolution(ItemData.Evolution.Condition.treasureChest)
        });

        GameObject secondWeaponObject = new GameObject("SecondWeapon");
        TestWeapon secondWeapon = secondWeaponObject.AddComponent<TestWeapon>();
        secondWeapon.attemptResult = false;
        secondWeapon.data = CreateWeaponData(new[]
        {
            CreateEvolution(ItemData.Evolution.Condition.treasureChest)
        });

        inventory.weaponSlots.Add(new PlayerInventory.Slot
        {
            item = firstWeapon
        });

        inventory.weaponSlots.Add(new PlayerInventory.Slot
        {
            item = secondWeapon
        });

        GameObject chestObject = new GameObject("Chest");
        TreasureChest chest = chestObject.AddComponent<TreasureChest>();

        chest.OpenTreasureChest(inventory, false);

        Assert.AreEqual(1, firstWeapon.attemptCalls);
        Assert.AreEqual(0, secondWeapon.attemptCalls);
    }

    [Test]
    public void OpenTreasureChest_WhenFirstWeaponEvolutionFails_ShouldContinueToNextWeapon()
    {
        PlayerInventory inventory = CreateInventory();

        GameObject firstWeaponObject = new GameObject("FirstWeapon");
        TestWeapon firstWeapon = firstWeaponObject.AddComponent<TestWeapon>();
        firstWeapon.attemptResult = false;
        firstWeapon.data = CreateWeaponData(new[]
        {
            CreateEvolution(ItemData.Evolution.Condition.treasureChest)
        });

        GameObject secondWeaponObject = new GameObject("SecondWeapon");
        TestWeapon secondWeapon = secondWeaponObject.AddComponent<TestWeapon>();
        secondWeapon.attemptResult = false;
        secondWeapon.data = CreateWeaponData(new[]
        {
            CreateEvolution(ItemData.Evolution.Condition.treasureChest)
        });

        inventory.weaponSlots.Add(new PlayerInventory.Slot
        {
            item = firstWeapon
        });

        inventory.weaponSlots.Add(new PlayerInventory.Slot
        {
            item = secondWeapon
        });

        GameObject chestObject = new GameObject("Chest");
        TreasureChest chest = chestObject.AddComponent<TreasureChest>();

        chest.OpenTreasureChest(inventory, false);

        Assert.AreEqual(1, firstWeapon.attemptCalls);
        Assert.AreEqual(1, secondWeapon.attemptCalls);
    }
}