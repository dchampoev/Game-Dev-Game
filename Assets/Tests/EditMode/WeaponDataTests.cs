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

        Weapon.LevelData result = data.GetLevelData(3);

        Assert.AreEqual(5f, ((Weapon.Stats) result).damage);
        Assert.AreEqual(7f, ((Weapon.Stats) result).speed);
        Assert.AreEqual(2, ((Weapon.Stats) result).number);

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

        Weapon.LevelData result = data.GetLevelData(2);

        Assert.AreEqual(9f, ((Weapon.Stats) result).damage);
        Assert.AreEqual(1.5f, ((Weapon.Stats) result).cooldown);
        Assert.AreEqual(4, ((Weapon.Stats) result).piercing);

        Object.DestroyImmediate(data);
    }

    [Test]
    public void GetLevelData_WhenNoGrowthConfigured_ShouldReturnDefaultStats()
    {
        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
        data.linearGrowth = new Weapon.Stats[0];
        data.randomGrowth = new Weapon.Stats[0];

        Weapon.LevelData result = data.GetLevelData(2);

        Assert.AreEqual(null, result.name);
        Assert.AreEqual(null, result.description);
        Assert.AreEqual(0f, ((Weapon.Stats) result).damage);
        Assert.AreEqual(0f, ((Weapon.Stats) result).cooldown);
        Assert.AreEqual(0f, ((Weapon.Stats) result).speed);
        Assert.AreEqual(0, ((Weapon.Stats) result).number);
        Assert.AreEqual(0, ((Weapon.Stats) result).piercing);

        Object.DestroyImmediate(data);
    }
}