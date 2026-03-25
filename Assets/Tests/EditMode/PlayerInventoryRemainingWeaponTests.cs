using System.Reflection;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        inventory.upgradeUIOptions = new List<PlayerInventory.UpgradeUI>();
        return inventory;
    }

    private PlayerInventory.UpgradeUI CreateUpgradeUI(string name)
    {
        GameObject container = new GameObject(name + "_Container");

        GameObject nameObject = new GameObject(name + "_Name");
        nameObject.transform.SetParent(container.transform);
        TextMeshProUGUI nameText = nameObject.AddComponent<TextMeshProUGUI>();

        GameObject descriptionObject = new GameObject(name + "_Description");
        descriptionObject.transform.SetParent(container.transform);
        TextMeshProUGUI descriptionText = descriptionObject.AddComponent<TextMeshProUGUI>();

        GameObject iconObject = new GameObject(name + "_Icon");
        iconObject.transform.SetParent(container.transform);
        Image icon = iconObject.AddComponent<Image>();

        GameObject buttonObject = new GameObject(name + "_Button");
        buttonObject.transform.SetParent(container.transform);
        buttonObject.AddComponent<Image>();
        Button button = buttonObject.AddComponent<Button>();

        return new PlayerInventory.UpgradeUI
        {
            upgradeNameDisplay = nameText,
            upgradeDescriptionDisplay = descriptionText,
            upgradeIcon = icon,
            upgradeButton = button
        };
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

    [Test]
    public void TakeRandomWeapon_WhenPoolIsEmpty_ShouldReturnNull()
    {
        PlayerInventory inventory = CreateInventory();
        List<WeaponData> weaponPool = new List<WeaponData>();

        WeaponData result = (WeaponData)InvokePrivate(inventory, "TakeRandomWeapon", weaponPool);

        Assert.IsNull(result);
    }

    [Test]
    public void TakeRandomWeapon_WhenPoolHasOneItem_ShouldReturnThatItemAndRemoveItFromPool()
    {
        PlayerInventory inventory = CreateInventory();
        WeaponData weaponData = CreateWeaponData("Knife");
        List<WeaponData> weaponPool = new List<WeaponData> { weaponData };

        WeaponData result = (WeaponData)InvokePrivate(inventory, "TakeRandomWeapon", weaponPool);

        Assert.AreSame(weaponData, result);
        Assert.AreEqual(0, weaponPool.Count);
    }

    [Test]
    public void ConfigureWeaponUpgradeOption_WhenPoolIsEmpty_ShouldDisableUi()
    {
        PlayerInventory inventory = CreateInventory();
        PlayerInventory.UpgradeUI ui = CreateUpgradeUI("Upgrade");
        List<WeaponData> weaponPool = new List<WeaponData>();

        ui.upgradeNameDisplay.transform.parent.gameObject.SetActive(true);

        InvokePrivate(inventory, "ConfigureWeaponUpgradeOption", ui, weaponPool);

        Assert.IsFalse(ui.upgradeNameDisplay.transform.parent.gameObject.activeSelf);
    }

    [Test]
    public void ConfigureWeaponUpgradeOption_WhenWeaponIsNew_ShouldPopulateUi()
    {
        PlayerInventory inventory = CreateInventory();
        PlayerInventory.UpgradeUI ui = CreateUpgradeUI("Upgrade");

        WeaponData weaponData = CreateWeaponData("Axe");
        List<WeaponData> weaponPool = new List<WeaponData> { weaponData };

        InvokePrivate(inventory, "ConfigureWeaponUpgradeOption", ui, weaponPool);

        Assert.IsTrue(ui.upgradeNameDisplay.transform.parent.gameObject.activeSelf);
        Assert.AreEqual("Axe", ui.upgradeNameDisplay.text);
        Assert.AreEqual("Axe_Description", ui.upgradeDescriptionDisplay.text);
        Assert.AreEqual(weaponData.icon, ui.upgradeIcon.sprite);
    }

    [Test]
    public void ConfigureNewWeaponPickup_ShouldPopulateUi()
    {
        PlayerInventory inventory = CreateInventory();
        PlayerInventory.UpgradeUI ui = CreateUpgradeUI("Upgrade");

        WeaponData weaponData = CreateWeaponData("Cross");

        InvokePrivate(inventory, "ConfigureNewWeaponPickup", ui, weaponData);

        Assert.AreEqual("Cross", ui.upgradeNameDisplay.text);
        Assert.AreEqual("Cross_Description", ui.upgradeDescriptionDisplay.text);
        Assert.AreEqual(weaponData.icon, ui.upgradeIcon.sprite);
    }
}