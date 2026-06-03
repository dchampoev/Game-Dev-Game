using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PowerUpTests
{
    PowerUpData CreatePowerUpData()
    {
        PowerUpData data = ScriptableObject.CreateInstance<PowerUpData>();
        data.name = "Test Power Up";
        data.maxLevel = 3;
        data.baseCost = 100f;
        data.baseFee = 20f;
        data.feeFactor = 1.5f;
        data.baseStats = new Passive.Modifier
        {
            name = "Base",
            description = "Base Description",
            boosts = new CharacterData.Stats
            {
                might = 1f
            }
        };
        data.growth = new Passive.Modifier[]
        {
            new Passive.Modifier
            {
                name = "Level 2",
                description = "Level 2 Description",
                boosts = new CharacterData.Stats
                {
                    might = 2f
                }
            },
            new Passive.Modifier
            {
                name = "Level 3",
                description = "Level 3 Description",
                boosts = new CharacterData.Stats
                {
                    might = 3f
                }
            }
        };
        return data;
    }

    [Test]
    public void Data_Constructor_ShouldStoreNameAndLevel()
    {
        PowerUp.Data data = new PowerUp.Data("Might", 2);

        Assert.AreEqual("Might", data.name);
        Assert.AreEqual(2, data.level);
    }

    [Test]
    public void GetCost_WhenNoBoughtLevelsOrFees_ShouldReturnBaseCost()
    {
        PowerUpData data = CreatePowerUpData();

        float cost = data.GetCost(1);

        Assert.AreEqual(100f, cost);
        Object.DestroyImmediate(data);
    }

    [Test]
    public void GetCost_WhenLevelAndTotalBoughtLevelsIncrease_ShouldIncludeBaseAndFeeCosts()
    {
        PowerUpData data = CreatePowerUpData();

        float cost = data.GetCost(3, 2);

        Assert.AreEqual(330f, cost);
        Object.DestroyImmediate(data);
    }

    [Test]
    public void GetBaseCost_WhenBoughtLevelsIsNegative_ShouldReturnZero()
    {
        PowerUpData data = CreatePowerUpData();

        float cost = data.GetBaseCost(-1);

        Assert.AreEqual(0f, cost);
        Object.DestroyImmediate(data);
    }

    [Test]
    public void GetBaseCost_WhenBoughtLevelsIsPositive_ShouldScaleFromBaseCost()
    {
        PowerUpData data = CreatePowerUpData();

        float cost = data.GetBaseCost(2);

        Assert.AreEqual(300f, cost);
        Object.DestroyImmediate(data);
    }

    [Test]
    public void GetFeeCost_WhenTotalBoughtLevelsIsZero_ShouldReturnZero()
    {
        PowerUpData data = CreatePowerUpData();

        float cost = data.GetFeeCost(0);

        Assert.AreEqual(0f, cost);
        Object.DestroyImmediate(data);
    }

    [Test]
    public void GetFeeCost_WhenTotalBoughtLevelsIsPositive_ShouldApplyFeeFactor()
    {
        PowerUpData data = CreatePowerUpData();

        float cost = data.GetFeeCost(3);

        Assert.AreEqual(45f, cost);
        Object.DestroyImmediate(data);
    }

    [Test]
    public void GetLevelData_WhenLevelIsOne_ShouldReturnBaseStats()
    {
        PowerUpData data = CreatePowerUpData();

        Item.LevelData levelData = data.GetLevelData(1);

        Assert.AreSame(data.baseStats, levelData);
        Assert.AreEqual("Base", levelData.name);
        Assert.AreEqual(1f, ((Passive.Modifier)levelData).boosts.might);
        Object.DestroyImmediate(data);
    }

    [Test]
    public void GetLevelData_WhenGrowthExists_ShouldReturnMatchingGrowthEntry()
    {
        PowerUpData data = CreatePowerUpData();

        Item.LevelData levelData = data.GetLevelData(3);

        Assert.AreSame(data.growth[1], levelData);
        Assert.AreEqual("Level 3", levelData.name);
        Assert.AreEqual(3f, ((Passive.Modifier)levelData).boosts.might);
        Object.DestroyImmediate(data);
    }

    [Test]
    public void GetLevelData_WhenGrowthIsMissing_ShouldReturnDefaultModifier()
    {
        PowerUpData data = CreatePowerUpData();

        LogAssert.Expect(LogType.Warning, "Power Up Test Power Up has no level data for level 4.");
        Item.LevelData levelData = data.GetLevelData(4);

        Assert.IsInstanceOf<Passive.Modifier>(levelData);
        Assert.IsNull(levelData.name);
        Assert.AreEqual(0f, ((Passive.Modifier)levelData).boosts.might);
        Object.DestroyImmediate(data);
    }
}
