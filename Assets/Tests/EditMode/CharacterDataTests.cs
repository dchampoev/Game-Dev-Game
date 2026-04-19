using NUnit.Framework;

public class CharacterDataTests
{
    [Test]
    public void StatsOperatorPlus_ShouldAddAllFields()
    {
        CharacterData.Stats left = new CharacterData.Stats
        {
            maxHealth = 10f,
            recovery = 1f,
            armor = 2f,
            moveSpeed = 2f,
            might = 3f,
            area = 4f,
            speed = 4f,
            duration = 5f,
            amount = 1,
            cooldown = 6f,
            luck = 7f,
            growth = 8f,
            greed = 9f,
            curse = 10f,
            magnet = 5f,
            revival = 1
        };

        CharacterData.Stats right = new CharacterData.Stats
        {
            maxHealth = 20f,
            recovery = 2f,
            armor = 3f,
            moveSpeed = 3f,
            might = 4f,
            area = 5f,
            speed = 5f,
            duration = 6f,
            amount = 2,
            cooldown = 7f,
            luck = 8f,
            growth = 9f,
            greed = 10f,
            curse = 11f,
            magnet = 6f,
            revival = 2
        };

        CharacterData.Stats result = left + right;

        Assert.AreEqual(30f, result.maxHealth);
        Assert.AreEqual(3f, result.recovery);
        Assert.AreEqual(5f, result.armor);
        Assert.AreEqual(5f, result.moveSpeed);
        Assert.AreEqual(7f, result.might);
        Assert.AreEqual(9f, result.area);
        Assert.AreEqual(9f, result.speed);
        Assert.AreEqual(11f, result.duration);
        Assert.AreEqual(3, result.amount);
        Assert.AreEqual(13f, result.cooldown);
        Assert.AreEqual(15f, result.luck);
        Assert.AreEqual(17f, result.growth);
        Assert.AreEqual(19f, result.greed);
        Assert.AreEqual(21f, result.curse);
        Assert.AreEqual(11f, result.magnet);
        Assert.AreEqual(3, result.revival);
    }

    [Test]
    public void StatsObjectInitializer_ShouldSetExpectedValues()
    {
        CharacterData.Stats stats = new CharacterData.Stats
        {
            maxHealth = 1000f,
            recovery = 0f,
            armor = 0f,
            moveSpeed = 1f,
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
            magnet = 2f,
            revival = 0
        };

        Assert.AreEqual(1000f, stats.maxHealth);
        Assert.AreEqual(0f, stats.recovery);
        Assert.AreEqual(0f, stats.armor);
        Assert.AreEqual(1f, stats.moveSpeed);
        Assert.AreEqual(1f, stats.might);
        Assert.AreEqual(1f, stats.area);
        Assert.AreEqual(1f, stats.speed);
        Assert.AreEqual(1f, stats.duration);
        Assert.AreEqual(0, stats.amount);
        Assert.AreEqual(1f, stats.cooldown);
        Assert.AreEqual(1f, stats.luck);
        Assert.AreEqual(1f, stats.growth);
        Assert.AreEqual(1f, stats.greed);
        Assert.AreEqual(0f, stats.curse);
        Assert.AreEqual(2f, stats.magnet);
        Assert.AreEqual(0, stats.revival);
    }
}