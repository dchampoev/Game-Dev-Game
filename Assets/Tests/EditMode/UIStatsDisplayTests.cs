using System.Collections.Generic;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIStatsDisplayTests
{
    private void SetPrivateField(object target, string fieldName, object value)
    {
        target.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(target, value);
    }

    private void CallPrivateMethod(object target, string methodName)
    {
        target.GetType()
            .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(target, null);
    }

    private PlayerStats CreatePlayer()
    {
        GameObject playerGO = new GameObject("Player");
        playerGO.SetActive(false);

        PlayerStats stats = playerGO.AddComponent<PlayerStats>();
        stats.enabled = false;

        PlayerMovement movement = playerGO.AddComponent<PlayerMovement>();
        movement.enabled = false;

        GameObject collectorGO = new GameObject("Collector");
        collectorGO.transform.SetParent(playerGO.transform);
        collectorGO.AddComponent<CircleCollider2D>();
        PlayerCollector collector = collectorGO.AddComponent<PlayerCollector>();
        collector.enabled = false;

        GameObject inventoryGO = new GameObject("Inventory");
        inventoryGO.transform.SetParent(playerGO.transform);
        PlayerInventory inventory = inventoryGO.AddComponent<PlayerInventory>();
        inventory.weaponSlots = new List<PlayerInventory.Slot>();
        inventory.passiveSlots = new List<PlayerInventory.Slot>();
        inventory.availableWeapons = new List<WeaponData>();
        inventory.availablePassives = new List<PassiveData>();

        GameObject healthBarGO = new GameObject("HealthBar");
        Image healthBar = healthBarGO.AddComponent<Image>();

        GameObject expBarGO = new GameObject("ExpBar");
        Image expBar = expBarGO.AddComponent<Image>();

        GameObject levelTextGO = new GameObject("LevelText");
        TextMeshProUGUI levelText = levelTextGO.AddComponent<TextMeshProUGUI>();

        stats.healthBar = healthBar;
        stats.expBar = expBar;
        stats.levelText = levelText;

        CharacterData characterData = ScriptableObject.CreateInstance<CharacterData>();
        CharacterData.Stats playerStats = new CharacterData.Stats
        {
            maxHealth = 20f,
            recovery = 1f,
            armor = 0f,
            moveSpeed = 5f,
            might = 1f,
            area = 1f,
            speed = 1f,
            duration = 1f,
            amount = 0,
            cooldown = 1f,
            luck = 1f,
            growth = 1f,
            greed = 1f,
            curse = 0f,
            magnet = 1f,
            revival = 0
        };

        stats.baseStats = playerStats;
        stats.Stats = playerStats;
        stats.CurrentHealth = 12f;

        SetPrivateField(stats, "characterData", characterData);
        SetPrivateField(stats, "inventory", inventory);
        SetPrivateField(stats, "collector", collector);
        SetPrivateField(stats, "health", 12f);

        return stats;
    }

    private UIStatsDisplay CreateDisplay(PlayerStats player = null)
    {
        GameObject displayGO = new GameObject("Display");
        UIStatsDisplay display = displayGO.AddComponent<UIStatsDisplay>();

        GameObject namesGO = new GameObject("Names");
        namesGO.transform.SetParent(displayGO.transform);
        namesGO.AddComponent<TextMeshProUGUI>();

        GameObject valuesGO = new GameObject("Values");
        valuesGO.transform.SetParent(displayGO.transform);
        valuesGO.AddComponent<TextMeshProUGUI>();

        display.player = player;
        return display;
    }

    private PlayerStats CreateActivePlayer()
    {
        PlayerStats player = CreatePlayer();
        player.gameObject.SetActive(true);
        return player;
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(obj);
        }

        foreach (var data in Resources.FindObjectsOfTypeAll<CharacterData>())
        {
            Object.DestroyImmediate(data, true);
        }
    }

    [Test]
    public void PrettifyNames_WhenCamelCase_ShouldInsertSpacesAndCapitalize()
    {
        StringBuilder input = new StringBuilder("maxHealth\nmoveSpeed\ncooldown\n");

        string result = UIStatsDisplay.PrettifyNames(input);

        Assert.AreEqual("Max Health\nMove Speed\nCooldown\n", result);
    }

    [Test]
    public void PrettifyNames_WhenEmpty_ShouldReturnEmptyString()
    {
        StringBuilder input = new StringBuilder();

        string result = UIStatsDisplay.PrettifyNames(input);

        Assert.AreEqual(string.Empty, result);
    }

    [Test]
    public void UpdateStatFields_WhenPlayerIsNull_ShouldLeaveTextsUnchanged()
    {
        UIStatsDisplay display = CreateDisplay(null);

        TextMeshProUGUI statNames = display.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI statValues = display.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        statNames.text = "OldNames";
        statValues.text = "OldValues";

        display.UpdateStatFields();

        Assert.AreEqual("OldNames", statNames.text);
        Assert.AreEqual("OldValues", statValues.text);
    }

    [Test]
    public void UpdateStatFields_WhenDisplayCurrentHealthIsTrue_ShouldIncludeHealthLine()
    {
        PlayerStats player = CreatePlayer();
        UIStatsDisplay display = CreateDisplay(player);
        display.displayCurrentHealth = true;

        TextMeshProUGUI statNames = display.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI statValues = display.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        display.UpdateStatFields();

        StringAssert.StartsWith("Health", statNames.text);
        StringAssert.StartsWith("12", statValues.text);
    }

    [Test]
    public void UpdateStatFields_WhenCalled_ShouldPopulateStatNames()
    {
        PlayerStats player = CreatePlayer();
        UIStatsDisplay display = CreateDisplay(player);

        TextMeshProUGUI statNames = display.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        display.UpdateStatFields();

        StringAssert.Contains("Max Health", statNames.text);
        StringAssert.Contains("Move Speed", statNames.text);
        StringAssert.Contains("Magnet", statNames.text);
    }

    [Test]
    public void Reset_WhenPlayerExists_ShouldAssignPlayer()
    {
        PlayerStats player = CreateActivePlayer();
        UIStatsDisplay display = CreateDisplay();

        CallPrivateMethod(display, "Reset");

        Assert.AreSame(player, display.player);
    }
}