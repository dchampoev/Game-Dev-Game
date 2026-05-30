using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class TreasureChestTests
{
    private class TestWeapon : Weapon
    {
        public int attemptCalls;
        public bool attemptResult;

        public override bool AttemptEvolution(ItemData.Evolution evolutionData, int levelUpAmount = 1, bool updateUI = true)
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
        data.maxLevel = 3;
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
        WeaponData outcomeData = ScriptableObject.CreateInstance<WeaponData>();
        outcomeData.baseStats = new Weapon.Stats
        {
            name = "Evolution Outcome",
            description = "Evolution Outcome"
        };
        outcomeData.linearGrowth = new Weapon.Stats[0];
        outcomeData.randomGrowth = new Weapon.Stats[0];

        return new ItemData.Evolution
        {
            evolutionLevel = 3,
            condition = condition,
            catalysts = new ItemData.Evolution.Config[0],
            outcome = new ItemData.Evolution.Config
            {
                itemType = outcomeData,
                level = 1
            }
        };
    }

    private TreasureChestDropProfile CreateDropProfile(int numberOfItems = 1)
    {
        TreasureChestDropProfile profile = ScriptableObject.CreateInstance<TreasureChestDropProfile>();
        profile.numberOfItems = numberOfItems;
        profile.baseDropChance = 100f;
        return profile;
    }

    private TreasureChest CreateChest(bool evolutionUnlocked = false)
    {
        GameObject chestObject = new GameObject("Chest");
        TreasureChest chest = chestObject.AddComponent<TreasureChest>();
        chest.possibleDrops = TreasureChest.DropType.Evolution;
        chest.dropProfiles = new[] { CreateDropProfile() };

        if (evolutionUnlocked)
        {
            chest.evolutionUnlockTime = 0f;
        }

        return chest;
    }

    private void SetFieldInHierarchy(object target, string fieldName, object value)
    {
        System.Type type = target.GetType();

        while (type != null)
        {
            FieldInfo field = type.GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
            );

            if (field != null)
            {
                field.SetValue(target, value);
                return;
            }

            type = type.BaseType;
        }

        Assert.Fail($"Field '{fieldName}' was not found.");
    }

    private void SetFieldOnType(System.Type type, object target, string fieldName, object value)
    {
        FieldInfo field = type.GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
        );

        Assert.NotNull(field, $"Field '{fieldName}' was not found on {type.Name}.");
        field.SetValue(target, value);
    }

    private void SetCurrentStats(Weapon weapon, Weapon.Stats stats)
    {
        SetFieldOnType(typeof(Weapon), weapon, "currentStats", stats);
    }

    private TestWeapon CreateWeapon(PlayerInventory inventory, ItemData.Evolution[] evolutions, bool attemptResult)
    {
        GameObject weaponObject = new GameObject("Weapon");
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();

        WeaponData weaponData = CreateWeaponData(evolutions);

        weapon.data = weaponData;
        weapon.maxLevel = weaponData.maxLevel;
        weapon.currentLevel = weaponData.maxLevel;
        weapon.attemptResult = attemptResult;

        SetFieldOnType(typeof(Item), weapon, "data", weaponData);
        SetFieldInHierarchy(weapon, "evolutionData", evolutions ?? new ItemData.Evolution[0]);
        SetCurrentStats(weapon, weaponData.baseStats);

        inventory.weaponSlots.Add(new PlayerInventory.Slot
        {
            item = weapon
        });

        return weapon;
    }

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(obj);
        }

        foreach (WeaponData data in Resources.FindObjectsOfTypeAll<WeaponData>())
        {
            Object.DestroyImmediate(data, true);
        }

        foreach (TreasureChestDropProfile data in Resources.FindObjectsOfTypeAll<TreasureChestDropProfile>())
        {
            Object.DestroyImmediate(data, true);
        }

        TreasureChest.totalPickups = 0;
    }

    [Test]
    public void OpenTreasureChest_WhenWeaponHasNoEvolutionData_ShouldSkipIt()
    {
        PlayerInventory inventory = CreateInventory();

        TestWeapon weapon = CreateWeapon(inventory, null, false);

        TreasureChest chest = CreateChest(true);

        chest.OpenTreasureChest(inventory, false);

        Assert.AreEqual(0, weapon.attemptCalls);
    }

    [Test]
    public void OpenTreasureChest_WhenEvolutionIsNotTreasureChest_ShouldNotAttemptEvolution()
    {
        PlayerInventory inventory = CreateInventory();

        TestWeapon weapon = CreateWeapon(inventory, new[]
        {
            CreateEvolution(ItemData.Evolution.Condition.auto)
        }, false);

        TreasureChest chest = CreateChest(true);

        chest.OpenTreasureChest(inventory, false);

        Assert.AreEqual(0, weapon.attemptCalls);
    }

    [Test]
    public void OpenTreasureChest_WhenTreasureChestEvolutionExists_ShouldAttemptEvolutionWithLevelUpAmountZero()
    {
        PlayerInventory inventory = CreateInventory();

        TestWeapon weapon = CreateWeapon(inventory, new[]
        {
            CreateEvolution(ItemData.Evolution.Condition.treasureChest)
        }, false);

        TreasureChest chest = CreateChest(true);

        chest.OpenTreasureChest(inventory, false);

        Assert.AreEqual(1, weapon.attemptCalls);
    }

    [Test]
    public void OpenTreasureChest_WhenFirstWeaponEvolutionSucceeds_ShouldReturnWithoutCheckingNextWeapon()
    {
        PlayerInventory inventory = CreateInventory();

        TestWeapon firstWeapon = CreateWeapon(inventory, new[]
        {
            CreateEvolution(ItemData.Evolution.Condition.treasureChest)
        }, true);

        TestWeapon secondWeapon = CreateWeapon(inventory, new[]
        {
            CreateEvolution(ItemData.Evolution.Condition.treasureChest)
        }, false);

        TreasureChest chest = CreateChest(true);

        chest.OpenTreasureChest(inventory, false);

        Assert.AreEqual(1, firstWeapon.attemptCalls);
        Assert.AreEqual(0, secondWeapon.attemptCalls);
    }

    [Test]
    public void OpenTreasureChest_WhenFirstWeaponEvolutionFails_ShouldContinueToNextWeapon()
    {
        PlayerInventory inventory = CreateInventory();

        TestWeapon firstWeapon = CreateWeapon(inventory, new[]
        {
            CreateEvolution(ItemData.Evolution.Condition.treasureChest)
        }, false);

        TestWeapon secondWeapon = CreateWeapon(inventory, new[]
        {
            CreateEvolution(ItemData.Evolution.Condition.treasureChest)
        }, false);

        TreasureChest chest = CreateChest(true);

        chest.OpenTreasureChest(inventory, false);

        Assert.AreEqual(1, firstWeapon.attemptCalls);
        Assert.AreEqual(1, secondWeapon.attemptCalls);
    }

    [Test]
    public void OpenTreasureChest_WhenEvolutionIsLocked_ShouldNotAttemptEvolution()
    {
        PlayerInventory inventory = CreateInventory();

        TestWeapon weapon = CreateWeapon(inventory, new[]
        {
            CreateEvolution(ItemData.Evolution.Condition.treasureChest)
        }, true);

        TreasureChest chest = CreateChest(false);

        chest.OpenTreasureChest(inventory, false);

        Assert.AreEqual(0, weapon.attemptCalls);
    }

    [Test]
    public void OpenTreasureChest_WhenEvolutionAlreadyAwarded_ShouldNotAwardSecondEvolution()
    {
        PlayerInventory inventory = CreateInventory();

        TestWeapon firstWeapon = CreateWeapon(inventory, new[]
        {
            CreateEvolution(ItemData.Evolution.Condition.treasureChest)
        }, true);

        TestWeapon secondWeapon = CreateWeapon(inventory, new[]
        {
            CreateEvolution(ItemData.Evolution.Condition.treasureChest)
        }, true);

        TreasureChest chest = CreateChest(true);

        chest.OpenTreasureChest(inventory, false);
        chest.OpenTreasureChest(inventory, false);

        Assert.AreEqual(1, firstWeapon.attemptCalls);
        Assert.AreEqual(0, secondWeapon.attemptCalls);
    }
}