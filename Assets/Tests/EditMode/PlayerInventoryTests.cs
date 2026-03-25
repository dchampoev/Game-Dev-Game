using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class PlayerInventoryTests
{
    private class TestWeapon : Weapon
    {
    }

    private class TestItemData : ItemData
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

    private PlayerInventory.Slot CreateEmptySlot(string name)
    {
        GameObject imageObject = new GameObject(name);
        Image image = imageObject.AddComponent<Image>();
        image.enabled = false;

        return new PlayerInventory.Slot
        {
            item = null,
            image = image
        };
    }

    private WeaponData CreateWeaponData(string name, string behaviour = null)
    {
        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
        data.name = name;
        data.maxLevel = 5;
        data.behaviour = behaviour;
        data.baseStats = new Weapon.Stats
        {
            name = name
        };
        data.linearGrowth = new Weapon.Stats[0];
        data.randomGrowth = new Weapon.Stats[0];
        return data;
    }

    private PassiveData CreatePassiveData(string name)
    {
        PassiveData data = ScriptableObject.CreateInstance<PassiveData>();
        data.name = name;
        data.maxLevel = 5;
        data.baseStats = new Passive.Modifier
        {
            name = name,
            description = name
        };
        data.growth = new Passive.Modifier[0];
        return data;
    }

    [Test]
    public void GetWeapon_WhenWeaponExistsInSlots_ShouldReturnWeapon()
    {
        PlayerInventory inventory = CreateInventory();

        WeaponData weaponData = CreateWeaponData("Knife");
        GameObject weaponObject = new GameObject("Weapon");
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();
        weapon.data = weaponData;

        inventory.weaponSlots.Add(new PlayerInventory.Slot
        {
            item = weapon,
            image = new GameObject("WeaponImage").AddComponent<Image>()
        });

        Weapon result = inventory.Get(weaponData);

        Assert.AreSame(weapon, result);
    }

    [Test]
    public void GetPassive_WhenPassiveExistsInSlots_ShouldReturnPassive()
    {
        PlayerInventory inventory = CreateInventory();

        PassiveData passiveData = CreatePassiveData("Spinach");
        GameObject passiveObject = new GameObject("Passive");
        Passive passive = passiveObject.AddComponent<Passive>();
        passive.data = passiveData;

        inventory.passiveSlots.Add(new PlayerInventory.Slot
        {
            item = passive,
            image = new GameObject("PassiveImage").AddComponent<Image>()
        });

        Passive result = inventory.Get(passiveData);

        Assert.AreSame(passive, result);
    }

    [Test]
    public void Has_WhenWeaponExists_ShouldReturnTrue()
    {
        PlayerInventory inventory = CreateInventory();

        WeaponData weaponData = CreateWeaponData("Whip");
        GameObject weaponObject = new GameObject("Weapon");
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();
        weapon.data = weaponData;

        inventory.weaponSlots.Add(new PlayerInventory.Slot
        {
            item = weapon,
            image = new GameObject("WeaponImage").AddComponent<Image>()
        });

        bool result = inventory.Has(weaponData);

        Assert.IsTrue(result);
    }

    [Test]
    public void Has_WhenItemDoesNotExist_ShouldReturnFalse()
    {
        PlayerInventory inventory = CreateInventory();
        WeaponData weaponData = CreateWeaponData("Axe");

        bool result = inventory.Has(weaponData);

        Assert.IsFalse(result);
    }

    [Test]
    public void AddWeapon_WhenBehaviourTypeIsMissing_ShouldReturnMinusOne()
    {
        PlayerInventory inventory = CreateInventory();
        inventory.weaponSlots.Add(CreateEmptySlot("WeaponSlot"));

        WeaponData weaponData = CreateWeaponData("BadWeapon", "Not.A.Real.Type");

        LogAssert.Expect(LogType.Warning, $"Weapon behaviour script {weaponData.behaviour} not found! Make sure the class name matches the string in WeaponData.");

        int result = inventory.Add(weaponData);

        Assert.AreEqual(-1, result);
        Assert.IsTrue(inventory.weaponSlots[0].IsEmpty());
    }

    [Test]
    public void AddItem_WhenUnsupportedItemDataType_ShouldReturnMinusOne()
    {
        PlayerInventory inventory = CreateInventory();

        TestItemData data = ScriptableObject.CreateInstance<TestItemData>();

        int result = inventory.Add(data);

        Assert.AreEqual(-1, result);
    }

    [Test]
    public void RemoveItem_WhenUnsupportedItemDataType_ShouldReturnFalse()
    {
        PlayerInventory inventory = CreateInventory();

        TestItemData data = ScriptableObject.CreateInstance<TestItemData>();

        bool result = inventory.Remove(data);

        Assert.IsFalse(result);
    }
}