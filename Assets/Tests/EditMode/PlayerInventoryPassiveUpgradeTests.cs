using System.Reflection;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInventoryPassiveUpgradeTests
{
    private object InvokePrivate(object target, string methodName, params object[] args)
    {
        return target.GetType()
            .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(target, args);
    }

    private class TestPassive : Passive
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

    private PassiveData CreatePassiveData(string name, int maxLevel = 5)
    {
        PassiveData data = ScriptableObject.CreateInstance<PassiveData>();
        data.name = name;
        data.maxLevel = maxLevel;
        data.baseStats = new Passive.Modifier
        {
            name = name,
            description = name + "_Base"
        };
        data.growth = new Passive.Modifier[]
        {
            new Passive.Modifier
            {
                name = name + "_L2",
                description = name + "_Desc_L2"
            },
            new Passive.Modifier
            {
                name = name + "_L3",
                description = name + "_Desc_L3"
            }
        };
        return data;
    }

    private void SetPrivateField(object target, string fieldName, object value)
    {
        target.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(target, value);
    }

    [Test]
    public void TryConfigureExistingPassiveLevelUp_WhenMatchingPassiveExists_ShouldReturnTrueAndPopulateUi()
    {
        PlayerInventory inventory = CreateInventory();
        PlayerInventory.UpgradeUI ui = CreateUpgradeUI("Upgrade");

        PassiveData passiveData = CreatePassiveData("Spinach", 5);

        GameObject passiveObject = new GameObject("Passive");
        TestPassive passive = passiveObject.AddComponent<TestPassive>();
        passive.data = passiveData;
        passive.currentLevel = 1;

        inventory.passiveSlots.Add(new PlayerInventory.Slot
        {
            item = passive,
            image = new GameObject("PassiveImage").AddComponent<Image>()
        });

        bool result = (bool)InvokePrivate(inventory, "TryConfigureExistingPassiveLevelUp", ui, passiveData);

        Assert.IsTrue(result);
        Assert.AreEqual("Spinach_L2", ui.upgradeNameDisplay.text);
        Assert.AreEqual("Spinach_Desc_L2", ui.upgradeDescriptionDisplay.text);
        Assert.AreEqual(passiveData.icon, ui.upgradeIcon.sprite);
    }

    [Test]
    public void TryConfigureExistingPassiveLevelUp_WhenPassiveIsAtMaxLevel_ShouldDisableUiAndReturnTrue()
    {
        PlayerInventory inventory = CreateInventory();
        PlayerInventory.UpgradeUI ui = CreateUpgradeUI("Upgrade");

        PassiveData passiveData = CreatePassiveData("Armor", 2);

        GameObject passiveObject = new GameObject("Passive");
        TestPassive passive = passiveObject.AddComponent<TestPassive>();
        passive.data = passiveData;
        passive.currentLevel = 2;

        inventory.passiveSlots.Add(new PlayerInventory.Slot
        {
            item = passive,
            image = new GameObject("PassiveImage").AddComponent<Image>()
        });

        ui.upgradeNameDisplay.transform.parent.gameObject.SetActive(true);

        bool result = (bool)InvokePrivate(inventory, "TryConfigureExistingPassiveLevelUp", ui, passiveData);

        Assert.IsTrue(result);
        Assert.IsFalse(ui.upgradeNameDisplay.transform.parent.gameObject.activeSelf);
    }

    [Test]
    public void TryConfigureExistingPassiveLevelUp_WhenMatchingPassiveDoesNotExist_ShouldReturnFalse()
    {
        PlayerInventory inventory = CreateInventory();
        PlayerInventory.UpgradeUI ui = CreateUpgradeUI("Upgrade");

        PassiveData passiveData = CreatePassiveData("Clover", 5);

        bool result = (bool)InvokePrivate(inventory, "TryConfigureExistingPassiveLevelUp", ui, passiveData);

        Assert.IsFalse(result);
    }

    [Test]
    public void BindPassiveLevelUp_WhenButtonClicked_ShouldCallLevelUpPassiveItem()
    {
        PlayerInventory inventory = CreateInventory();
        PlayerInventory.UpgradeUI ui = CreateUpgradeUI("Upgrade");

        GameObject playerObject = new GameObject("Player");
        PlayerStats playerStats = playerObject.AddComponent<PlayerStats>();

        SetPrivateField(inventory, "player", playerStats);
        SetPrivateField(playerStats, "inventory", inventory);
        SetPrivateField(playerStats, "characterData", ScriptableObject.CreateInstance<CharacterData>());

        PassiveData passiveData = CreatePassiveData("HollowHeart", 5);

        GameObject passiveObject = new GameObject("Passive");
        TestPassive passive = passiveObject.AddComponent<TestPassive>();
        passive.data = passiveData;
        passive.maxLevel = passiveData.maxLevel;
        passive.currentLevel = 1;

        inventory.passiveSlots.Add(new PlayerInventory.Slot
        {
            item = passive,
            image = new GameObject("PassiveImage").AddComponent<Image>()
        });

        InvokePrivate(inventory, "BindPassiveLevelUp", ui, 0);

        ui.upgradeButton.onClick.Invoke();

        Assert.AreEqual(2, passive.currentLevel);
    }
}