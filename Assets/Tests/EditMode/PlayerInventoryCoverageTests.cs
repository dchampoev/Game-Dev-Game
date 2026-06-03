using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerInventoryCoverageTests
{
    class TestWeapon : Weapon
    {
        public override void Initialize(WeaponData data)
        {
            this.data = data;
            maxLevel = data.maxLevel;
        }

        public void SetEvolutionContext(PlayerInventory playerInventory, ItemData.Evolution[] evolutions)
        {
            inventory = playerInventory;
            evolutionData = evolutions;
        }
    }

    class TestItem : Item
    {
    }

    class TestItemData : ItemData
    {
        public override Item.LevelData GetLevelData(int level)
        {
            return new Item.LevelData();
        }
    }

    PlayerInventory CreateInventory()
    {
        GameObject go = new GameObject("Inventory");
        PlayerInventory inventory = go.AddComponent<PlayerInventory>();
        inventory.weaponSlots = new List<PlayerInventory.Slot>();
        inventory.passiveSlots = new List<PlayerInventory.Slot>();
        inventory.powerUps = new List<PowerUp>();
        inventory.availableWeapons = new List<WeaponData>();
        inventory.availablePassives = new List<PassiveData>();
        inventory.availablePowerUps = new List<PowerUpData>();
        return inventory;
    }

    PlayerInventory.Slot CreateSlot(Item item)
    {
        return new PlayerInventory.Slot
        {
            item = item
        };
    }

    WeaponData CreateWeaponData(string itemName = "Weapon", int maxLevel = 3)
    {
        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
        data.name = itemName;
        data.maxLevel = maxLevel;
        data.behaviour = typeof(TestWeapon).AssemblyQualifiedName;
        data.baseStats = new Weapon.Stats
        {
            name = itemName
        };
        data.linearGrowth = new Weapon.Stats[0];
        data.randomGrowth = new Weapon.Stats[0];
        return data;
    }

    PassiveData CreatePassiveData(string itemName = "Passive", int maxLevel = 3)
    {
        PassiveData data = ScriptableObject.CreateInstance<PassiveData>();
        data.name = itemName;
        data.maxLevel = maxLevel;
        data.baseStats = new Passive.Modifier
        {
            name = itemName
        };
        data.growth = new Passive.Modifier[0];
        return data;
    }

    PowerUpData CreatePowerUpData(string itemName = "Power Up", int maxLevel = 3)
    {
        PowerUpData data = ScriptableObject.CreateInstance<PowerUpData>();
        data.name = itemName;
        data.maxLevel = maxLevel;
        data.baseStats = new Passive.Modifier
        {
            name = itemName
        };
        data.growth = new Passive.Modifier[]
        {
            new Passive.Modifier
            {
                name = itemName + " Level 2"
            },
            new Passive.Modifier
            {
                name = itemName + " Level 3"
            }
        };
        return data;
    }

    TestWeapon CreateWeapon(WeaponData data, int currentLevel = 1)
    {
        TestWeapon weapon = new GameObject(data.name).AddComponent<TestWeapon>();
        weapon.data = data;
        weapon.currentLevel = currentLevel;
        weapon.maxLevel = data.maxLevel;
        return weapon;
    }

    Passive CreatePassive(PassiveData data, int currentLevel = 1)
    {
        Passive passive = new GameObject(data.name).AddComponent<Passive>();
        passive.data = data;
        passive.currentLevel = currentLevel;
        passive.maxLevel = data.maxLevel;
        return passive;
    }

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(obj);
        }

        TestScriptableObjectCleanup.DestroyRuntimeObjects<WeaponData>();
        TestScriptableObjectCleanup.DestroyRuntimeObjects<PassiveData>();
        TestScriptableObjectCleanup.DestroyRuntimeObjects<PowerUpData>();
        TestScriptableObjectCleanup.DestroyRuntimeObjects<TestItemData>();
    }

    [Test]
    public void AddPowerUp_WhenDataIsNull_ShouldReturnMinusOne()
    {
        PlayerInventory inventory = CreateInventory();

        int result = inventory.Add((PowerUpData)null);

        Assert.AreEqual(-1, result);
        Assert.IsEmpty(inventory.powerUps);
    }

    [Test]
    public void AddPowerUp_WhenDataIsValid_ShouldCreatePowerUpAtRequestedLevel()
    {
        PlayerInventory inventory = CreateInventory();
        PowerUpData powerUpData = CreatePowerUpData(maxLevel: 3);

        int result = inventory.Add(powerUpData, 3);

        Assert.AreEqual(1, result);
        Assert.AreEqual(1, inventory.powerUps.Count);
        Assert.AreSame(powerUpData, inventory.powerUps[0].data);
        Assert.AreEqual(3, inventory.powerUps[0].currentLevel);
        Assert.AreSame(inventory.powerUps[0], inventory.Get(powerUpData));
    }

    [Test]
    public void AddPowerUpSaveData_ShouldAddMatchingConfiguredPowerUp()
    {
        PlayerInventory inventory = CreateInventory();
        PowerUpData powerUpData = CreatePowerUpData("Might");
        inventory.availablePowerUps.Add(powerUpData);

        bool result = inventory.Add(new PowerUp.Data("Might", 2));

        Assert.IsTrue(result);
        Assert.AreEqual(1, inventory.powerUps.Count);
        Assert.AreEqual(2, inventory.powerUps[0].currentLevel);
    }

    [Test]
    public void AddPowerUpSaveData_WhenDataIsNullOrMissing_ShouldReturnFalse()
    {
        PlayerInventory inventory = CreateInventory();

        Assert.IsFalse(inventory.Add((PowerUp.Data)null));
        Assert.IsFalse(inventory.Add(new PowerUp.Data("Missing", 1)));
    }

    [Test]
    public void RemovePowerUp_WhenPowerUpDoesNotExist_ShouldReturnFalse()
    {
        PlayerInventory inventory = CreateInventory();
        PowerUpData powerUpData = CreatePowerUpData();

        Assert.IsFalse(inventory.Remove(powerUpData));
    }

    [Test]
    public void RemoveWeaponOrPassive_WhenRemoveUpgradeAvailabilityIsTrue_ShouldRemoveAvailabilityEvenIfNotOwned()
    {
        PlayerInventory inventory = CreateInventory();
        WeaponData weaponData = CreateWeaponData("Axe");
        PassiveData passiveData = CreatePassiveData("Spinach");
        inventory.availableWeapons.Add(weaponData);
        inventory.availablePassives.Add(passiveData);

        Assert.IsFalse(inventory.Remove(weaponData, true));
        Assert.IsFalse(inventory.Remove(passiveData, true));

        Assert.IsFalse(inventory.availableWeapons.Contains(weaponData));
        Assert.IsFalse(inventory.availablePassives.Contains(passiveData));
    }

    [Test]
    public void AddWeapon_WhenSlotIsAvailable_ShouldCreateWeaponAndAssignSlot()
    {
        PlayerInventory inventory = CreateInventory();
        inventory.weaponSlots.Add(CreateSlot(null));
        WeaponData weaponData = CreateWeaponData("Knife");

        int result = inventory.Add(weaponData, false);

        Assert.AreEqual(0, result);
        Assert.IsNotNull(inventory.weaponSlots[0].item);
        Assert.AreSame(weaponData, inventory.weaponSlots[0].item.data);
    }

    [Test]
    public void AddWeapon_WhenNoSlotIsAvailable_ShouldReturnMinusOne()
    {
        PlayerInventory inventory = CreateInventory();
        WeaponData weaponData = CreateWeaponData("Knife");

        int result = inventory.Add(weaponData);

        Assert.AreEqual(-1, result);
    }

    [Test]
    public void AddPassive_WhenSlotIsAvailable_ShouldCreatePassiveAndAssignSlot()
    {
        PlayerInventory inventory = CreateInventory();
        inventory.passiveSlots.Add(CreateSlot(null));
        PassiveData passiveData = CreatePassiveData("Armor");

        int result = inventory.Add(passiveData, false);

        Assert.AreEqual(0, result);
        Assert.IsNotNull(inventory.passiveSlots[0].item);
        Assert.AreSame(passiveData, inventory.passiveSlots[0].item.data);
    }

    [Test]
    public void AddPassive_WhenNoSlotIsAvailable_ShouldReturnMinusOne()
    {
        PlayerInventory inventory = CreateInventory();
        PassiveData passiveData = CreatePassiveData("Armor");

        int result = inventory.Add(passiveData);

        Assert.AreEqual(-1, result);
    }

    [Test]
    public void GetSlots_ShouldReturnMatchingSlotGroupsAndWarnForInvalidTypes()
    {
        PlayerInventory inventory = CreateInventory();
        inventory.weaponSlots.Add(CreateSlot(null));
        inventory.passiveSlots.Add(CreateSlot(null));

        Assert.AreEqual(1, inventory.GetSlots<Weapon>().Length);
        Assert.AreEqual(1, inventory.GetSlots<Passive>().Length);
        Assert.IsNull(inventory.GetSlots<PowerUp>());
        Assert.AreEqual(2, inventory.GetSlots<Item>().Length);

        LogAssert.Expect(LogType.Warning, $"Invalid type parameter {typeof(TestItem)} in GetSlots<T>()");
        Assert.IsNull(inventory.GetSlots<TestItem>());
    }

    [Test]
    public void GetSlotsFor_ShouldReturnMatchingSlotGroupsAndWarnForInvalidTypes()
    {
        PlayerInventory inventory = CreateInventory();
        inventory.weaponSlots.Add(CreateSlot(null));
        inventory.passiveSlots.Add(CreateSlot(null));

        Assert.AreEqual(1, inventory.GetSlotsFor<WeaponData>().Length);
        Assert.AreEqual(1, inventory.GetSlotsFor<PassiveData>().Length);
        Assert.IsNull(inventory.GetSlotsFor<PowerUpData>());
        Assert.AreEqual(2, inventory.GetSlotsFor<ItemData>().Length);

        LogAssert.Expect(LogType.Warning, $"Invalid type parameter {typeof(TestItemData)} in GetSlotsFor<T>()");
        Assert.IsNull(inventory.GetSlotsFor<TestItemData>());
    }

    [Test]
    public void GetAvailable_ShouldReturnConfiguredListsAndWarnForInvalidTypes()
    {
        PlayerInventory inventory = CreateInventory();
        WeaponData weaponData = CreateWeaponData();
        PassiveData passiveData = CreatePassiveData();
        PowerUpData powerUpData = CreatePowerUpData();
        inventory.availableWeapons.Add(weaponData);
        inventory.availablePassives.Add(passiveData);
        inventory.availablePowerUps.Add(powerUpData);

        Assert.AreEqual(1, inventory.GetAvailable<WeaponData>().Length);
        Assert.AreEqual(1, inventory.GetAvailable<PassiveData>().Length);
        Assert.AreEqual(1, inventory.GetAvailable<PowerUpData>().Length);
        Assert.AreEqual(2, inventory.GetAvailable<ItemData>().Length);

        LogAssert.Expect(LogType.Warning, $"Invalid type parameter {typeof(TestItemData)} in GetAvailable<T>()");
        Assert.IsNull(inventory.GetAvailable<TestItemData>());
    }

    [Test]
    public void GetUnowned_WhenAvailableIsEmpty_ShouldReturnEmptyArray()
    {
        PlayerInventory inventory = CreateInventory();

        Assert.IsEmpty(inventory.GetUnowned<WeaponData>());
    }

    [Test]
    public void GetUnowned_WhenItemIsOwned_ShouldFilterOwnedItem()
    {
        PlayerInventory inventory = CreateInventory();
        PassiveData ownedPassiveData = CreatePassiveData("Owned");
        PassiveData unownedPassiveData = CreatePassiveData("Unowned");
        inventory.availablePassives.Add(ownedPassiveData);
        inventory.availablePassives.Add(unownedPassiveData);
        inventory.passiveSlots.Add(CreateSlot(CreatePassive(ownedPassiveData)));

        PassiveData[] result = inventory.GetUnowned<PassiveData>();

        Assert.AreEqual(1, result.Length);
        Assert.AreSame(unownedPassiveData, result[0]);
    }

    [Test]
    public void GetSlotsLeftGeneric_ShouldReturnEmptySlotCounts()
    {
        PlayerInventory inventory = CreateInventory();
        inventory.weaponSlots.Add(CreateSlot(null));
        inventory.weaponSlots.Add(CreateSlot(CreateWeapon(CreateWeaponData())));
        inventory.passiveSlots.Add(CreateSlot(null));

        Assert.AreEqual(1, inventory.GetSlotsLeft<Weapon>());
        Assert.AreEqual(1, inventory.GetSlotsLeftFor<PassiveData>());
        Assert.AreEqual(0, inventory.GetSlotsLeft<PowerUp>());
    }

    [Test]
    public void GetUpgradables_ShouldReturnItemsThatCanLevelUp()
    {
        PlayerInventory inventory = CreateInventory();
        WeaponData upgradableData = CreateWeaponData("Upgradable", maxLevel: 3);
        WeaponData maxedData = CreateWeaponData("Maxed", maxLevel: 1);
        TestWeapon upgradable = CreateWeapon(upgradableData, currentLevel: 1);
        TestWeapon maxed = CreateWeapon(maxedData, currentLevel: 1);
        inventory.weaponSlots.Add(CreateSlot(upgradable));
        inventory.weaponSlots.Add(CreateSlot(maxed));

        Weapon[] result = inventory.GetUpgradables<Weapon>();

        Assert.AreEqual(1, result.Length);
        Assert.AreSame(upgradable, result[0]);
        Assert.IsEmpty(inventory.GetUpgradables<PowerUp>());
    }

    [Test]
    public void GetEvolvables_ShouldReturnItemsThatCanEvolve()
    {
        PlayerInventory inventory = CreateInventory();
        WeaponData weaponData = CreateWeaponData("Weapon", maxLevel: 3);
        WeaponData evolutionData = CreateWeaponData("Evolution");
        weaponData.evolutionData = new ItemData.Evolution[]
        {
            new ItemData.Evolution
            {
                evolutionLevel = 1,
                condition = ItemData.Evolution.Condition.treasureChest,
                outcome = new ItemData.Evolution.Config
                {
                    itemType = evolutionData,
                    level = 1
                }
            }
        };

        TestWeapon weapon = CreateWeapon(weaponData, currentLevel: 1);
        weapon.SetEvolutionContext(inventory, weaponData.evolutionData);
        inventory.weaponSlots.Add(CreateSlot(weapon));

        Weapon[] result = inventory.GetEvolvables<Weapon>();

        Assert.AreEqual(1, result.Length);
        Assert.AreSame(weapon, result[0]);
        Assert.IsEmpty(inventory.GetEvolvables<PowerUp>());
    }
}
