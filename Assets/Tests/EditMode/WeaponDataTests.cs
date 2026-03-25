using NUnit.Framework;
using UnityEngine;

public class WeaponDataTests
{
    [Test]
    public void GetLevelData_WhenLinearGrowthExists_ShouldReturnCorrectEntry()
    {
        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
        data.linearGrowth = new Weapon.Stats[]
        {
            new Weapon.Stats { damage = 2f, speed = 3f, number = 1 },
            new Weapon.Stats { damage = 5f, speed = 7f, number = 2 }
        };
        data.randomGrowth = new Weapon.Stats[0];

        Weapon.Stats result = data.GetLevelData(3);

        Assert.AreEqual(5f, result.damage);
        Assert.AreEqual(7f, result.speed);
        Assert.AreEqual(2, result.number);

        Object.DestroyImmediate(data);
    }

    [Test]
    public void GetLevelData_WhenOnlyOneRandomGrowthExists_ShouldReturnThatEntry()
    {
        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
        data.linearGrowth = new Weapon.Stats[0];
        data.randomGrowth = new Weapon.Stats[]
        {
            new Weapon.Stats { damage = 9f, cooldown = 1.5f, piercing = 4 }
        };

        Weapon.Stats result = data.GetLevelData(2);

        Assert.AreEqual(9f, result.damage);
        Assert.AreEqual(1.5f, result.cooldown);
        Assert.AreEqual(4, result.piercing);

        Object.DestroyImmediate(data);
    }

    [Test]
    public void GetLevelData_WhenNoGrowthConfigured_ShouldReturnDefaultStats()
    {
        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
        data.linearGrowth = new Weapon.Stats[0];
        data.randomGrowth = new Weapon.Stats[0];

        Weapon.Stats result = data.GetLevelData(2);

        Assert.AreEqual(null, result.name);
        Assert.AreEqual(null, result.description);
        Assert.AreEqual(0f, result.damage);
        Assert.AreEqual(0f, result.cooldown);
        Assert.AreEqual(0f, result.speed);
        Assert.AreEqual(0, result.number);
        Assert.AreEqual(0, result.piercing);

        Object.DestroyImmediate(data);
    }
}