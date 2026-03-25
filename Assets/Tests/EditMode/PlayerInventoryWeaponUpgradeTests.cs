using System.Reflection;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInventoryWeaponUpgradeTests
{
    private object InvokePrivate(object target, string methodName, params object[] args)
    {
        return target.GetType()
            .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(target, args);
    }

    private void SetPrivateField(object target, string fieldName, object value)
    {
        target.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(target, value);
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

    [Test]
    public void TryConfigureExistingWeaponLevelUp_WhenMatchingWeaponExists_ShouldReturnTrueAndPopulateUi()
    {
        PlayerInventory inventory = CreateInventory();
        PlayerInventory.UpgradeUI ui = CreateUpgradeUI("Upgrade");

        WeaponData weaponData = CreateWeaponData("Knife", 5);

        GameObject weaponObject = new GameObject("Weapon");
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();
        weapon.data = weaponData;
        weapon.maxLevel = weaponData.maxLevel;
        weapon.currentLevel = 1;

        inventory.weaponSlots.Add(new PlayerInventory.Slot
        {
            item = weapon,
            image = new GameObject("WeaponImage").AddComponent<Image>()
        });

        bool result = (bool)InvokePrivate(inventory, "TryConfigureExistingWeaponLevelUp", ui, weaponData);

        Assert.IsTrue(result);
        Assert.AreEqual("Knife_L2", ui.upgradeNameDisplay.text);
        Assert.AreEqual("Knife_Desc_L2", ui.upgradeDescriptionDisplay.text);
        Assert.AreEqual(weaponData.icon, ui.upgradeIcon.sprite);
    }

    [Test]
    public void TryConfigureExistingWeaponLevelUp_WhenWeaponIsAtMaxLevel_ShouldDisableUiAndReturnTrue()
    {
        PlayerInventory inventory = CreateInventory();
        PlayerInventory.UpgradeUI ui = CreateUpgradeUI("Upgrade");

        WeaponData weaponData = CreateWeaponData("Whip", 2);

        GameObject weaponObject = new GameObject("Weapon");
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();
        weapon.data = weaponData;
        weapon.maxLevel = weaponData.maxLevel;
        weapon.currentLevel = 2;

        inventory.weaponSlots.Add(new PlayerInventory.Slot
        {
            item = weapon,
            image = new GameObject("WeaponImage").AddComponent<Image>()
        });

        ui.upgradeNameDisplay.transform.parent.gameObject.SetActive(true);

        bool result = (bool)InvokePrivate(inventory, "TryConfigureExistingWeaponLevelUp", ui, weaponData);

        Assert.IsTrue(result);
        Assert.IsFalse(ui.upgradeNameDisplay.transform.parent.gameObject.activeSelf);
    }

    [Test]
    public void TryConfigureExistingWeaponLevelUp_WhenMatchingWeaponDoesNotExist_ShouldReturnFalse()
    {
        PlayerInventory inventory = CreateInventory();
        PlayerInventory.UpgradeUI ui = CreateUpgradeUI("Upgrade");

        WeaponData weaponData = CreateWeaponData("Axe", 5);

        bool result = (bool)InvokePrivate(inventory, "TryConfigureExistingWeaponLevelUp", ui, weaponData);

        Assert.IsFalse(result);
    }

    [Test]
    public void BindWeaponLevelUp_WhenButtonClicked_ShouldCallLevelUpWeapon()
    {
        PlayerInventory inventory = CreateInventory();
        PlayerInventory.UpgradeUI ui = CreateUpgradeUI("Upgrade");

        GameObject playerObject = new GameObject("Player");
        PlayerStats playerStats = playerObject.AddComponent<PlayerStats>();

        SetPrivateField(inventory, "player", playerStats);
        SetPrivateField(playerStats, "inventory", inventory);
        SetPrivateField(playerStats, "characterData", ScriptableObject.CreateInstance<CharacterData>());

        WeaponData weaponData = CreateWeaponData("MagicWand", 5);

        GameObject weaponObject = new GameObject("Weapon");
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();
        weapon.data = weaponData;
        weapon.maxLevel = weaponData.maxLevel;
        weapon.currentLevel = 1;

        inventory.weaponSlots.Add(new PlayerInventory.Slot
        {
            item = weapon,
            image = new GameObject("WeaponImage").AddComponent<Image>()
        });

        InvokePrivate(inventory, "BindWeaponLevelUp", ui, 0);

        ui.upgradeButton.onClick.Invoke();

        Assert.AreEqual(2, weapon.currentLevel);
    }
}