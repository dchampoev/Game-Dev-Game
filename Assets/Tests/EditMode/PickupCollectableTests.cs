using NUnit.Framework;
using UnityEngine;

public class PickupCollectableTests
{
    private PlayerStats playerStats;
    private GameObject playerObject;

    [SetUp]
    public void Setup()
    {
        playerObject = new GameObject("Player");
        playerStats = playerObject.AddComponent<PlayerStats>();
        playerObject.tag = "Player";

        playerStats.levelRanges = new System.Collections.Generic.List<PlayerStats.LevelRange>
        {
            new PlayerStats.LevelRange { startLevel = 1, endLevel = 10, experienceCapIncrease = 10 }
        };

        playerStats.level = 1;
        playerStats.experience = 0;
        playerStats.experienceCap = 10;
        playerStats.CurrentHealth = 50f;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(playerObject);
    }

    [Test]
    public void ExperienceGem_Collect_ShouldIncreaseExperience()
    {
        GameObject gemObject = new GameObject("Gem");
        ExperienceGem gem = gemObject.AddComponent<ExperienceGem>();
        gem.experienceGranted = 5;

        gem.Collect();

        Assert.AreEqual(5, playerStats.experience);

        Object.DestroyImmediate(gemObject);
    }

    [Test]
    public void ExperienceGem_Collect_WhenExperienceReachesCap_ShouldLevelUpPlayer()
    {
        GameObject gemObject = new GameObject("Gem");
        ExperienceGem gem = gemObject.AddComponent<ExperienceGem>();
        gem.experienceGranted = 10;

        gem.Collect();

        Assert.AreEqual(2, playerStats.level);
        Assert.AreEqual(0, playerStats.experience);

        Object.DestroyImmediate(gemObject);
    }
}