using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIUpgradeWindowTests
{
    private class TestItemData : ItemData
    {
        public string levelName;
        public string levelDescription;

        public override Item.LevelData GetLevelData(int level)
        {
            return new Item.LevelData
            {
                name = levelName,
                description = levelDescription
            };
        }
    }

    private class TestItem : Item
    {
        public ItemData data;

        public override void Initialize(ItemData data)
        {
            this.data = data;
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

    private RectTransform CreateOptionTemplate()
    {
        GameObject option = new GameObject("Upgrade Option");
        RectTransform rect = option.AddComponent<RectTransform>();

        GameObject icon = new GameObject("Icon");
        icon.transform.SetParent(option.transform);
        GameObject itemIcon = new GameObject("Item Icon");
        itemIcon.transform.SetParent(icon.transform);
        itemIcon.AddComponent<Image>();

        GameObject name = new GameObject("Name");
        name.transform.SetParent(option.transform);
        name.AddComponent<TextMeshProUGUI>();

        GameObject description = new GameObject("Description");
        description.transform.SetParent(option.transform);
        description.AddComponent<TextMeshProUGUI>();

        GameObject level = new GameObject("Level");
        level.transform.SetParent(option.transform);
        level.AddComponent<TextMeshProUGUI>();

        GameObject button = new GameObject("Button");
        button.transform.SetParent(option.transform);
        button.AddComponent<Image>();
        button.AddComponent<Button>();

        return rect;
    }

    private UIUpgradeWindow CreateWindow()
    {
        GameObject windowObject = new GameObject("Upgrade Window");
        RectTransform windowRect = windowObject.AddComponent<RectTransform>();
        windowRect.sizeDelta = new Vector2(400f, 400f);
        windowObject.AddComponent<VerticalLayoutGroup>();

        RectTransform optionTemplate = CreateOptionTemplate();
        optionTemplate.name = "Upgrade Option";
        optionTemplate.SetParent(windowObject.transform);

        GameObject tooltipObject = new GameObject("Tooltip");
        tooltipObject.transform.SetParent(windowObject.transform);
        TextMeshProUGUI tooltip = tooltipObject.AddComponent<TextMeshProUGUI>();

        UIUpgradeWindow window = windowObject.AddComponent<UIUpgradeWindow>();

        window.upgradeOptionTemplate = optionTemplate;
        window.tooltipTemplate = tooltip;
        window.maxOptions = 4;

        typeof(UIUpgradeWindow)
            .GetMethod("Awake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            .Invoke(window, null);

        return window;
    }

    private TestItemData CreateItemData(string name, string levelName, string description)
    {
        TestItemData data = ScriptableObject.CreateInstance<TestItemData>();
        data.name = name;
        data.maxLevel = 5;
        data.levelName = levelName;
        data.levelDescription = description;
        return data;
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(obj);
        }

        foreach (var data in Resources.FindObjectsOfTypeAll<TestItemData>())
        {
            Object.DestroyImmediate(data, true);
        }
    }

    [Test]
    public void SetUpgrades_WhenNewItem_ShouldPopulateUi()
    {
        UIUpgradeWindow window = CreateWindow();
        PlayerInventory inventory = CreateInventory();

        TestItemData itemData = CreateItemData("Spinach", "Base", "Spinach_Description");
        List<ItemData> upgrades = new List<ItemData> { itemData };

        window.SetUpgrades(inventory, upgrades, 1);

        Transform option = window.upgradeOptionTemplate.transform;

        Assert.IsTrue(option.gameObject.activeSelf);
        Assert.AreEqual("Spinach", option.Find("Name").GetComponent<TextMeshProUGUI>().text);
        Assert.AreEqual("New!", option.Find("Level").GetComponent<TextMeshProUGUI>().text);
        Assert.AreEqual("Spinach_Description", option.Find("Description").GetComponent<TextMeshProUGUI>().text);
        Assert.AreEqual(itemData.icon, option.Find("Icon/Item Icon").GetComponent<Image>().sprite);
    }

    [Test]
    public void SetUpgrades_WhenTooltipIsProvided_ShouldShowTooltip()
    {
        UIUpgradeWindow window = CreateWindow();
        PlayerInventory inventory = CreateInventory();

        TestItemData itemData = CreateItemData("Clover", "Base", "Clover_Description");
        List<ItemData> upgrades = new List<ItemData> { itemData };

        window.SetUpgrades(inventory, upgrades, 1, "Luck tooltip");

        Assert.IsTrue(window.tooltipTemplate.gameObject.activeSelf);
        Assert.AreEqual("Luck tooltip", window.tooltipTemplate.text);
    }

    [Test]
    public void SetUpgrades_WhenNoTooltip_ShouldHideTooltip()
    {
        UIUpgradeWindow window = CreateWindow();
        PlayerInventory inventory = CreateInventory();

        TestItemData itemData = CreateItemData("Clover", "Base", "Clover_Description");
        List<ItemData> upgrades = new List<ItemData> { itemData };

        window.SetUpgrades(inventory, upgrades, 1, "");

        Assert.IsFalse(window.tooltipTemplate.gameObject.activeSelf);
    }

    [Test]
    public void SetUpgrades_WhenNoItems_ShouldDisableOption()
    {
        UIUpgradeWindow window = CreateWindow();
        PlayerInventory inventory = CreateInventory();

        window.SetUpgrades(inventory, new List<ItemData>(), 1);

        Assert.IsFalse(window.upgradeOptionTemplate.gameObject.activeSelf);
    }
}