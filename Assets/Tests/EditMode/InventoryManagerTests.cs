using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManagerTests
{
    private void SetPrivateField(object obj, string fieldName, object value)
    {
        obj.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(obj, value);
    }

    private T InvokePrivateMethod<T>(object obj, string methodName, params object[] args)
    {
        return (T)obj.GetType()
            .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(obj, args);
    }

    private void InvokePrivateMethod(object obj, string methodName, params object[] args)
    {
        obj.GetType()
            .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(obj, args);
    }

    private Sprite CreateSprite()
    {
        Texture2D texture = new Texture2D(2, 2);
        return Sprite.Create(texture, new Rect(0, 0, 2, 2), Vector2.one * 0.5f);
    }

    private WeaponScriptableObject CreateWeaponData(
        string weaponName = "Weapon",
        string description = "Desc",
        int level = 1,
        Sprite icon = null,
        GameObject nextLevelPrefab = null,
        int evolvedUpgradeToRemove = 0)
    {
        WeaponScriptableObject data = ScriptableObject.CreateInstance<WeaponScriptableObject>();

        SetPrivateField(data, "name", weaponName);
        SetPrivateField(data, "description", description);
        SetPrivateField(data, "level", level);
        SetPrivateField(data, "icon", icon);
        SetPrivateField(data, "nextLevelPrefab", nextLevelPrefab);
        SetPrivateField(data, "evolvedUpgradeToRemove", evolvedUpgradeToRemove);
        SetPrivateField(data, "damage", 1f);
        SetPrivateField(data, "speed", 1f);
        SetPrivateField(data, "cooldownDuration", 1f);
        SetPrivateField(data, "pierce", 1);

        return data;
    }

    private PassiveItemScriptableObject CreatePassiveItemData(
        string itemName = "Passive",
        string description = "Desc",
        int level = 1,
        Sprite icon = null,
        GameObject nextLevelPrefab = null)
    {
        PassiveItemScriptableObject data = ScriptableObject.CreateInstance<PassiveItemScriptableObject>();

        SetPrivateField(data, "name", itemName);
        SetPrivateField(data, "description", description);
        SetPrivateField(data, "level", level);
        SetPrivateField(data, "icon", icon);
        SetPrivateField(data, "nextLevelPrefab", nextLevelPrefab);
        SetPrivateField(data, "multiplier", 1f);

        return data;
    }

    private WeaponController CreateWeaponController(
        string weaponName,
        int level,
        Sprite icon,
        GameObject nextLevelPrefab = null,
        int evolvedUpgradeToRemove = 0)
    {
        GameObject go = new GameObject(weaponName);
        WeaponController controller = go.AddComponent<WeaponController>();
        controller.weaponData = CreateWeaponData(
            weaponName,
            weaponName + " desc",
            level,
            icon,
            nextLevelPrefab,
            evolvedUpgradeToRemove);

        return controller;
    }

    private PassiveItem CreatePassiveItem(
        string itemName,
        int level,
        Sprite icon,
        GameObject nextLevelPrefab = null)
    {
        GameObject go = new GameObject(itemName);
        PassiveItem passiveItem = go.AddComponent<PassiveItem>();
        passiveItem.passiveItemData = CreatePassiveItemData(
            itemName,
            itemName + " desc",
            level,
            icon,
            nextLevelPrefab);

        return passiveItem;
    }

    private InventoryManager CreateInventoryManager()
    {
        GameObject player = new GameObject("Player");
        InventoryManager inventory = player.AddComponent<InventoryManager>();

        for (int i = 0; i < 6; i++)
        {
            inventory.weaponSlots.Add(null);
            inventory.passiveItemSlots.Add(null);

            GameObject weaponUiGo = new GameObject($"WeaponUI_{i}");
            Image weaponUi = weaponUiGo.AddComponent<Image>();
            weaponUi.enabled = false;
            inventory.weaponUISlots.Add(weaponUi);

            GameObject passiveUiGo = new GameObject($"PassiveUI_{i}");
            Image passiveUi = passiveUiGo.AddComponent<Image>();
            passiveUi.enabled = false;
            inventory.passiveItemUISlots.Add(passiveUi);
        }

        return inventory;
    }

    private InventoryManager.UpgradeUI CreateUpgradeUI()
    {
        GameObject root = new GameObject("UpgradeRoot");
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(root.transform);
        panel.SetActive(true);

        GameObject textRoot = new GameObject("TextRoot");
        textRoot.transform.SetParent(panel.transform);

        TextMeshProUGUI nameText = textRoot.AddComponent<TextMeshProUGUI>();

        GameObject descGo = new GameObject("Desc");
        descGo.transform.SetParent(panel.transform);
        TextMeshProUGUI descText = descGo.AddComponent<TextMeshProUGUI>();

        GameObject iconGo = new GameObject("Icon");
        iconGo.transform.SetParent(panel.transform);
        Image icon = iconGo.AddComponent<Image>();

        GameObject buttonGo = new GameObject("Button");
        buttonGo.transform.SetParent(panel.transform);
        Button button = buttonGo.AddComponent<Button>();

        return new InventoryManager.UpgradeUI
        {
            upgradeNameDisplay = nameText,
            upgradeDescriptionDisplay = descText,
            upgradeIcon = icon,
            upgradeButton = button
        };
    }

    private WeaponEvolutionBlueprint CreateEvolutionBlueprint(
        WeaponScriptableObject weaponData,
        PassiveItemScriptableObject passiveData,
        GameObject evolvedWeapon,
        WeaponScriptableObject evolvedWeaponData = null)
    {
        WeaponEvolutionBlueprint blueprint = ScriptableObject.CreateInstance<WeaponEvolutionBlueprint>();
        blueprint.weaponToEvolveData = weaponData;
        blueprint.catalystPassiveItemData = passiveData;
        blueprint.evolvedWeapon = evolvedWeapon;
        blueprint.evolvedWeaponData = evolvedWeaponData;
        return blueprint;
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(go);
        }

        foreach (var weaponData in Resources.FindObjectsOfTypeAll<WeaponScriptableObject>())
        {
            Object.DestroyImmediate(weaponData);
        }

        foreach (var passiveData in Resources.FindObjectsOfTypeAll<PassiveItemScriptableObject>())
        {
            Object.DestroyImmediate(passiveData);
        }

        foreach (var blueprint in Resources.FindObjectsOfTypeAll<WeaponEvolutionBlueprint>())
        {
            Object.DestroyImmediate(blueprint);
        }
    }

    [Test]
    public void AddWeapon_ShouldSetSlotLevelAndUI()
    {
        InventoryManager inventory = CreateInventoryManager();
        Sprite icon = CreateSprite();
        WeaponController weapon = CreateWeaponController("Knife", 2, icon);

        inventory.AddWeapon(0, weapon);

        Assert.AreEqual(weapon, inventory.weaponSlots[0]);
        Assert.AreEqual(2, inventory.weaponLevels[0]);
        Assert.IsTrue(inventory.weaponUISlots[0].enabled);
        Assert.AreEqual(icon, inventory.weaponUISlots[0].sprite);
    }

    [Test]
    public void AddPassiveItem_ShouldSetSlotLevelAndUI()
    {
        InventoryManager inventory = CreateInventoryManager();
        Sprite icon = CreateSprite();
        PassiveItem passiveItem = CreatePassiveItem("Armor", 3, icon);

        inventory.AddPassiveItem(0, passiveItem);

        Assert.AreEqual(passiveItem, inventory.passiveItemSlots[0]);
        Assert.AreEqual(3, inventory.passiveItemsLevels[0]);
        Assert.IsTrue(inventory.passiveItemUISlots[0].enabled);
        Assert.AreEqual(icon, inventory.passiveItemUISlots[0].sprite);
    }

    [Test]
    public void DetermineUpgradeType_WhenNoPassiveUpgrades_ShouldReturnWeaponType()
    {
        InventoryManager inventory = CreateInventoryManager();

        List<InventoryManager.WeaponUpgrade> weaponUpgrades = new List<InventoryManager.WeaponUpgrade>
        {
            new InventoryManager.WeaponUpgrade()
        };

        List<InventoryManager.PassiveItemUpgrade> passiveUpgrades = new List<InventoryManager.PassiveItemUpgrade>();

        int result = InvokePrivateMethod<int>(inventory, "DetermineUpgradeType", weaponUpgrades, passiveUpgrades);

        Assert.AreEqual(1, result);
    }

    [Test]
    public void DetermineUpgradeType_WhenNoWeaponUpgrades_ShouldReturnPassiveType()
    {
        InventoryManager inventory = CreateInventoryManager();

        List<InventoryManager.WeaponUpgrade> weaponUpgrades = new List<InventoryManager.WeaponUpgrade>();
        List<InventoryManager.PassiveItemUpgrade> passiveUpgrades = new List<InventoryManager.PassiveItemUpgrade>
        {
            new InventoryManager.PassiveItemUpgrade()
        };

        int result = InvokePrivateMethod<int>(inventory, "DetermineUpgradeType", weaponUpgrades, passiveUpgrades);

        Assert.AreEqual(2, result);
    }

    [Test]
    public void GetRandomWeaponUpgrade_ShouldReturnElementAndRemoveItFromList()
    {
        InventoryManager inventory = CreateInventoryManager();

        InventoryManager.WeaponUpgrade upgrade = new InventoryManager.WeaponUpgrade
        {
            weaponUpgradeIndex = 7
        };

        List<InventoryManager.WeaponUpgrade> upgrades = new List<InventoryManager.WeaponUpgrade> { upgrade };

        InventoryManager.WeaponUpgrade result =
            InvokePrivateMethod<InventoryManager.WeaponUpgrade>(inventory, "GetRandomWeaponUpgrade", upgrades);

        Assert.AreEqual(upgrade, result);
        Assert.AreEqual(0, upgrades.Count);
    }

    [Test]
    public void GetRandomPassiveItemUpgrade_ShouldReturnElementAndRemoveItFromList()
    {
        InventoryManager inventory = CreateInventoryManager();

        InventoryManager.PassiveItemUpgrade upgrade = new InventoryManager.PassiveItemUpgrade
        {
            passiveItemUpgradeIndex = 9
        };

        List<InventoryManager.PassiveItemUpgrade> upgrades = new List<InventoryManager.PassiveItemUpgrade> { upgrade };

        InventoryManager.PassiveItemUpgrade result =
            InvokePrivateMethod<InventoryManager.PassiveItemUpgrade>(inventory, "GetRandomPassiveItemUpgrade", upgrades);

        Assert.AreEqual(upgrade, result);
        Assert.AreEqual(0, upgrades.Count);
    }

    [Test]
    public void FindWeaponSlotIndex_WhenWeaponExists_ShouldReturnCorrectIndex()
    {
        InventoryManager inventory = CreateInventoryManager();
        Sprite icon = CreateSprite();

        WeaponController weapon = CreateWeaponController("Axe", 1, icon);
        inventory.weaponSlots[3] = weapon;

        int result = InvokePrivateMethod<int>(inventory, "FindWeaponSlotIndex", weapon.weaponData);

        Assert.AreEqual(3, result);
    }

    [Test]
    public void FindPassiveItemSlotIndex_WhenPassiveItemExists_ShouldReturnCorrectIndex()
    {
        InventoryManager inventory = CreateInventoryManager();
        Sprite icon = CreateSprite();

        PassiveItem passiveItem = CreatePassiveItem("Clover", 1, icon);
        inventory.passiveItemSlots[4] = passiveItem;

        int result = InvokePrivateMethod<int>(inventory, "FindPassiveItemSlotIndex", passiveItem.passiveItemData);

        Assert.AreEqual(4, result);
    }

    [Test]
    public void EnableAndDisableUpgradeUI_ShouldToggleParentGameObject()
    {
        InventoryManager inventory = CreateInventoryManager();
        InventoryManager.UpgradeUI ui = CreateUpgradeUI();

        InvokePrivateMethod(inventory, "DisableUpgradeUI", ui);
        Assert.IsFalse(ui.upgradeNameDisplay.transform.parent.gameObject.activeSelf);

        InvokePrivateMethod(inventory, "EnableUpgradeUI", ui);
        Assert.IsTrue(ui.upgradeNameDisplay.transform.parent.gameObject.activeSelf);
    }

    [Test]
    public void GetPossibleEvolutions_WhenRequirementsAreMet_ShouldReturnEvolution()
    {
        InventoryManager inventory = CreateInventoryManager();
        Sprite weaponIcon = CreateSprite();
        Sprite passiveIcon = CreateSprite();
        Sprite evolvedIcon = CreateSprite();

        WeaponController weapon = CreateWeaponController("Whip", 3, weaponIcon);
        PassiveItem passive = CreatePassiveItem("Heart", 2, passiveIcon);
        WeaponController evolvedWeapon = CreateWeaponController("Bloody Tear", 5, evolvedIcon);

        inventory.weaponSlots[0] = weapon;
        inventory.passiveItemSlots[0] = passive;

        WeaponEvolutionBlueprint blueprint = CreateEvolutionBlueprint(
            weapon.weaponData,
            passive.passiveItemData,
            evolvedWeapon.gameObject,
            evolvedWeapon.weaponData);

        inventory.weaponEvolutions.Add(blueprint);

        List<WeaponEvolutionBlueprint> result = inventory.GetPossibleEvolutions();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(blueprint, result[0]);
    }
    [Test]
    public void RemoveUpgradeOptions_ShouldDisableAllUpgradeUIs()
    {
        InventoryManager inventory = CreateInventoryManager();

        InventoryManager.UpgradeUI ui1 = CreateUpgradeUI();
        InventoryManager.UpgradeUI ui2 = CreateUpgradeUI();

        inventory.upgradeUIOptions.Add(ui1);
        inventory.upgradeUIOptions.Add(ui2);

        ui1.upgradeNameDisplay.transform.parent.gameObject.SetActive(true);
        ui2.upgradeNameDisplay.transform.parent.gameObject.SetActive(true);

        ui1.upgradeButton.onClick.AddListener(() => { });
        ui2.upgradeButton.onClick.AddListener(() => { });

        InvokePrivateMethod(inventory, "RemoveUpgradeOptions");

        Assert.IsFalse(ui1.upgradeNameDisplay.transform.parent.gameObject.activeSelf);
        Assert.IsFalse(ui2.upgradeNameDisplay.transform.parent.gameObject.activeSelf);
    }
}