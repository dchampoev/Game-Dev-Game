using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatsTests
{
    private void SetPrivateField(object obj, string fieldName, object value)
    {
        obj.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(obj, value);
    }

    private void SetCharacterData(PlayerStats stats, CharacterData value)
    {
        typeof(PlayerStats)
            .GetField("characterData", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(stats, value);
    }

    private void SetPrivateBool(PlayerStats stats, string fieldName, bool value)
    {
        typeof(PlayerStats)
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(stats, value);
    }

    private void SetPrivateFloat(PlayerStats stats, string fieldName, float value)
    {
        typeof(PlayerStats)
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(stats, value);
    }

    private T GetPrivateField<T>(object obj, string fieldName)
    {
        return (T)obj.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(obj);
    }

    private void CallPrivateMethod(object obj, string methodName)
    {
        obj.GetType()
            .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(obj, null);
    }

    private PlayerStats CreatePlayer()
    {
        GameObject playerGO = new GameObject("Player");
        playerGO.SetActive(false);

        PlayerStats stats = playerGO.AddComponent<PlayerStats>();

        GameObject healthBarGO = new GameObject("HealthBar");
        Image healthBar = healthBarGO.AddComponent<Image>();

        GameObject expBarGO = new GameObject("ExpBar");
        Image expBar = expBarGO.AddComponent<Image>();

        GameObject levelTextGO = new GameObject("LevelText");
        TextMeshProUGUI levelText = levelTextGO.AddComponent<TextMeshProUGUI>();

        stats.healthBar = healthBar;
        stats.expBar = expBar;
        stats.levelText = levelText;

        CharacterData.Stats startStats = new CharacterData.Stats
        {
            maxHealth = 20f,
            recovery = 1f,
            moveSpeed = 5f,
            might = 1f,
            speed = 1f,
            magnet = 1f
        };

        CharacterData characterData = ScriptableObject.CreateInstance<CharacterData>();

        stats.baseStats = startStats;
        SetPrivateField(stats, "actualStats", startStats);
        SetPrivateField(stats, "health", 20f);
        SetPrivateField(stats, "inventory", playerGO.AddComponent<PlayerInventory>());
        SetCharacterData(stats, characterData);

        stats.level = 1;
        stats.experience = 0;
        stats.experienceCap = 10;
        stats.iFrameDuration = 0.1f;
        stats.levelRanges = new List<PlayerStats.LevelRange>
        {
            new PlayerStats.LevelRange
            {
                startLevel = 1,
                endLevel = 10,
                experienceCapIncrease = 5
            }
        };

        return stats;
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
            Object.DestroyImmediate(data);
        }
    }

    [Test]
    public void TakeDamage_ShouldReduceHealth()
    {
        PlayerStats stats = CreatePlayer();
        float initialHealth = stats.CurrentHealth;

        stats.TakeDamage(5f);

        Assert.Less(stats.CurrentHealth, initialHealth);
    }

    [Test]
    public void TakeDamage_WhenInvincible_ShouldNotReduceHealthAgain()
    {
        PlayerStats stats = CreatePlayer();

        stats.TakeDamage(5f);
        float afterFirstHit = stats.CurrentHealth;

        stats.TakeDamage(5f);

        Assert.AreEqual(afterFirstHit, stats.CurrentHealth);
    }

    [Test]
    public void Heal_ShouldIncreaseHealth()
    {
        PlayerStats stats = CreatePlayer();
        stats.CurrentHealth = 5f;

        stats.Heal(3f);

        Assert.AreEqual(8f, stats.CurrentHealth);
    }

    [Test]
    public void Heal_ShouldNotExceedMaxHealth()
    {
        PlayerStats stats = CreatePlayer();
        stats.CurrentHealth = stats.MaxHealth - 1f;

        stats.Heal(10f);

        Assert.AreEqual(stats.MaxHealth, stats.CurrentHealth);
    }

    [Test]
    public void Heal_WhenCharacterDataIsNull_ShouldReturnWithoutChangingHealth()
    {
        PlayerStats stats = CreatePlayer();
        stats.CurrentHealth = 5f;
        SetCharacterData(stats, null);

        stats.Heal(3f);

        Assert.AreEqual(5f, stats.CurrentHealth);
    }

    [Test]
    public void IncreaseExperience_ShouldIncreaseExperience()
    {
        PlayerStats stats = CreatePlayer();
        int initialExp = stats.experience;

        stats.IncreaseExperience(5);

        Assert.Greater(stats.experience, initialExp);
        Assert.AreEqual(5, stats.experience);
    }

    [Test]
    public void IncreaseExperience_WhenReachingCap_ShouldLevelUp()
    {
        PlayerStats stats = CreatePlayer();
        stats.experience = 9;
        stats.experienceCap = 10;

        stats.IncreaseExperience(1);

        Assert.AreEqual(2, stats.level);
        Assert.AreEqual(0, stats.experience);
        Assert.AreEqual(15, stats.experienceCap);
    }

    [Test]
    public void UpdateHealthBar_ShouldReflectCurrentHealth()
    {
        PlayerStats stats = CreatePlayer();
        stats.CurrentHealth = 10f;

        CallPrivateMethod(stats, "UpdateHealthBar");

        Assert.AreEqual(0.5f, stats.healthBar.fillAmount, 0.01f);
    }

    [Test]
    public void UpdateExpBar_ShouldReflectCurrentExperience()
    {
        PlayerStats stats = CreatePlayer();
        stats.experience = 5;
        stats.experienceCap = 10;

        CallPrivateMethod(stats, "UpdateExpBar");

        Assert.AreEqual(0.5f, stats.expBar.fillAmount, 0.01f);
    }

    [Test]
    public void UpdateLevelText_ShouldShowLevelPrefix()
    {
        PlayerStats stats = CreatePlayer();
        stats.level = 5;

        CallPrivateMethod(stats, "UpdateLevelText");

        Assert.AreEqual("LV 5", stats.levelText.text);
    }

    [Test]
    public void Recover_ShouldIncreaseHealthWhenBelowMax()
    {
        PlayerStats stats = CreatePlayer();
        stats.CurrentHealth = 10f;

        CallPrivateMethod(stats, "Recover");

        Assert.Greater(stats.CurrentHealth, 10f);
        Assert.LessOrEqual(stats.CurrentHealth, stats.MaxHealth);
    }

    [Test]
    public void Recover_ShouldNotExceedMaxHealth()
    {
        PlayerStats stats = CreatePlayer();
        stats.CurrentHealth = stats.MaxHealth - 0.01f;

        CallPrivateMethod(stats, "Recover");

        Assert.LessOrEqual(stats.CurrentHealth, stats.MaxHealth);
    }
}