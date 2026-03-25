using System.Reflection;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInventoryUpgradeUiTests
{
    private object InvokePrivate(object target, string methodName, params object[] args)
    {
        return target.GetType()
            .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(target, args);
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

    private PassiveData CreatePassiveData(string name)
    {
        PassiveData data = ScriptableObject.CreateInstance<PassiveData>();
        data.name = name;
        data.maxLevel = 5;
        data.baseStats = new Passive.Modifier
        {
            name = name,
            description = name + "_Description"
        };
        data.growth = new Passive.Modifier[0];
        return data;
    }

    [Test]
    public void DisableUpgradeUI_ShouldDeactivateParentObject()
    {
        PlayerInventory inventory = CreateInventory();
        PlayerInventory.UpgradeUI ui = CreateUpgradeUI("Upgrade");

        ui.upgradeNameDisplay.transform.parent.gameObject.SetActive(true);

        InvokePrivate(inventory, "DisableUpgradeUI", ui);

        Assert.IsFalse(ui.upgradeNameDisplay.transform.parent.gameObject.activeSelf);
    }

    [Test]
    public void EnableUpgradeUI_ShouldActivateParentObject()
    {
        PlayerInventory inventory = CreateInventory();
        PlayerInventory.UpgradeUI ui = CreateUpgradeUI("Upgrade");

        ui.upgradeNameDisplay.transform.parent.gameObject.SetActive(false);

        InvokePrivate(inventory, "EnableUpgradeUI", ui);

        Assert.IsTrue(ui.upgradeNameDisplay.transform.parent.gameObject.activeSelf);
    }

    [Test]
    public void RemoveUpgradeOptions_ShouldDisableUiAndRemoveAllButtonListeners()
    {
        PlayerInventory inventory = CreateInventory();
        PlayerInventory.UpgradeUI ui = CreateUpgradeUI("Upgrade");
        inventory.upgradeUIOptions.Add(ui);

        int clickCount = 0;
        ui.upgradeButton.onClick.AddListener(() => clickCount++);
        ui.upgradeNameDisplay.transform.parent.gameObject.SetActive(true);

        InvokePrivate(inventory, "RemoveUpgradeOptions");

        ui.upgradeButton.onClick.Invoke();

        Assert.AreEqual(0, clickCount);
        Assert.IsFalse(ui.upgradeNameDisplay.transform.parent.gameObject.activeSelf);
    }

    [Test]
    public void RemoveAndApplyUpgrades_WhenPassivePoolHasItem_ShouldEnableUiAndPopulateContent()
    {
        PlayerInventory inventory = CreateInventory();
        PlayerInventory.UpgradeUI ui = CreateUpgradeUI("Upgrade");
        inventory.upgradeUIOptions.Add(ui);

        PassiveData passiveData = CreatePassiveData("Spinach");
        inventory.availablePassives.Add(passiveData);

        ui.upgradeNameDisplay.transform.parent.gameObject.SetActive(true);

        inventory.RemoveAndApplyUpgrades();

        Assert.IsTrue(ui.upgradeNameDisplay.transform.parent.gameObject.activeSelf);
        Assert.AreEqual("Spinach", ui.upgradeNameDisplay.text);
        Assert.AreEqual("Spinach_Description", ui.upgradeDescriptionDisplay.text);
        Assert.AreEqual(passiveData.icon, ui.upgradeIcon.sprite);
    }
}