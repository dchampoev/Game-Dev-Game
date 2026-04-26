using NUnit.Framework;
using UnityEngine;

public class PassiveDataTests
{
    [Test]
    public void GetLevelData_WhenGrowthExists_ShouldReturnCorrectEntry()
    {
        PassiveData data = ScriptableObject.CreateInstance<PassiveData>();
        data.growth = new Passive.Modifier[]
        {
            new Passive.Modifier
            {
                name = "Level2",
                description = "First",
                boosts = new CharacterData.Stats
                {
                    might = 1f
                }
            },
            new Passive.Modifier
            {
                name = "Level3",
                description = "Second",
                boosts = new CharacterData.Stats
                {
                    might = 2f
                }
            }
        };

        Passive.LevelData result = data.GetLevelData(3);

        Assert.AreEqual("Level3", result.name);
        Assert.AreEqual("Second", result.description);
        Assert.AreEqual(2f, ((Passive.Modifier)result).boosts.might);

        Object.DestroyImmediate(data);
    }

    [Test]
    public void GetLevelData_WhenGrowthMissing_ShouldReturnDefaultModifier()
    {
        PassiveData data = ScriptableObject.CreateInstance<PassiveData>();
        data.growth = new Passive.Modifier[0];

        Passive.LevelData result = data.GetLevelData(2);

        Assert.IsNull(result.name);
        Assert.IsNull(result.description);
        Assert.AreEqual(0f, ((Passive.Modifier)result).boosts.maxHealth);
        Assert.AreEqual(0f, ((Passive.Modifier)result).boosts.recovery);
        Assert.AreEqual(0f, ((Passive.Modifier)result).boosts.armor);
        Assert.AreEqual(0f, ((Passive.Modifier)result).boosts.moveSpeed);
        Assert.AreEqual(0f, ((Passive.Modifier)result).boosts.might);
        Assert.AreEqual(0f, ((Passive.Modifier)result).boosts.area);
        Assert.AreEqual(0f, ((Passive.Modifier)result).boosts.speed);
        Assert.AreEqual(0f, ((Passive.Modifier)result).boosts.duration);
        Assert.AreEqual(0, ((Passive.Modifier)result).boosts.amount);
        Assert.AreEqual(0f, ((Passive.Modifier)result).boosts.cooldown);
        Assert.AreEqual(0f, ((Passive.Modifier)result).boosts.luck);
        Assert.AreEqual(0f, ((Passive.Modifier)result).boosts.growth);
        Assert.AreEqual(0f, ((Passive.Modifier)result).boosts.greed);
        Assert.AreEqual(0f, ((Passive.Modifier)result).boosts.curse);
        Assert.AreEqual(0f, ((Passive.Modifier)result).boosts.magnet);
        Assert.AreEqual(0, ((Passive.Modifier)result).boosts.revival);

        Object.DestroyImmediate(data);
    }
}