using NUnit.Framework;

public class CharacterDataTests
{
    [Test]
    public void StatsOperatorPlus_ShouldAddAllFields()
    {
        CharacterData.Stats left = new CharacterData.Stats(
            maxHealth: 10f,
            recovery: 1f,
            moveSpeed: 2f,
            might: 3f,
            speed: 4f,
            magnet: 5f
        );

        CharacterData.Stats right = new CharacterData.Stats(
            maxHealth: 20f,
            recovery: 2f,
            moveSpeed: 3f,
            might: 4f,
            speed: 5f,
            magnet: 6f
        );

        CharacterData.Stats result = left + right;

        Assert.AreEqual(30f, result.maxHealth);
        Assert.AreEqual(3f, result.recovery);
        Assert.AreEqual(5f, result.moveSpeed);
        Assert.AreEqual(7f, result.might);
        Assert.AreEqual(9f, result.speed);
        Assert.AreEqual(11f, result.magnet);
    }

    [Test]
    public void StatsConstructor_WithNoArguments_ShouldSetExpectedDefaults()
    {
        CharacterData.Stats stats = new CharacterData.Stats(1000f, 0f, 1f, 1f, 1f, 2f);

        Assert.AreEqual(1000f, stats.maxHealth);
        Assert.AreEqual(0f, stats.recovery);
        Assert.AreEqual(1f, stats.moveSpeed);
        Assert.AreEqual(1f, stats.might);
        Assert.AreEqual(1f, stats.speed);
        Assert.AreEqual(2f, stats.magnet);
    }
}