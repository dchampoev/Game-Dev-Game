using NUnit.Framework;
using UnityEngine;

public class HealthPotionTests
{
    [Test]
    public void Collect_WhenPlayerExists_ShouldNotThrow()
    {
        GameObject playerObject = new GameObject("Player");
        PlayerStats playerStats = playerObject.AddComponent<PlayerStats>();

        playerStats.CurrentHealth = 50f;

        GameObject potionObject = new GameObject("Potion");
        HealthPotion potion = potionObject.AddComponent<HealthPotion>();
        potion.healthToRestore = 20;

        Assert.DoesNotThrow(() => potion.Collect());

        Object.DestroyImmediate(potionObject);
        Object.DestroyImmediate(playerObject);
    }

    [Test]
    public void Collect_WhenPlayerDoesNotExist_ShouldNotThrow()
    {
        GameObject potionObject = new GameObject("Potion");
        HealthPotion potion = potionObject.AddComponent<HealthPotion>();
        potion.healthToRestore = 20;

        Assert.DoesNotThrow(() => potion.Collect());

        Object.DestroyImmediate(potionObject);
    }
}