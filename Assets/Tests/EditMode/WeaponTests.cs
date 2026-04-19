using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class WeaponTests
{
    private class TestWeapon : Weapon
    {
    }

    private T GetField<T>(object target, string fieldName)
    {
        System.Type type = target.GetType();

        while (type != null)
        {
            FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field != null)
            {
                return (T)field.GetValue(target);
            }

            type = type.BaseType;
        }

        return default;
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(obj);
        }

        foreach (var data in Resources.FindObjectsOfTypeAll<WeaponData>())
        {
            Object.DestroyImmediate(data, true);
        }
    }

    [Test]
    public void StatsGetDamage_WhenDamageVarianceIsZero_ShouldReturnBaseDamage()
    {
        Weapon.Stats stats = new Weapon.Stats
        {
            damage = 7f,
            damageVariance = 0f
        };

        float result = stats.GetDamage();

        Assert.AreEqual(7f, result);
    }

    [Test]
    public void Initialize_ShouldSetDataStatsAndMovement()
    {
        GameObject playerObject = new GameObject("Player");
        playerObject.AddComponent<PlayerInventory>();
        PlayerMovement movement = playerObject.AddComponent<PlayerMovement>();
        movement.enabled = false;

        PlayerStats playerStats = playerObject.AddComponent<PlayerStats>();
        playerStats.enabled = false;

        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
        data.maxLevel = 5;
        data.baseStats = new Weapon.Stats
        {
            damage = 3f,
            cooldown = 1.5f
        };
        data.linearGrowth = new Weapon.Stats[0];
        data.randomGrowth = new Weapon.Stats[0];

        GameObject weaponObject = new GameObject("Weapon");
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();

        weapon.Initialize(data);

        Weapon.Stats currentStats = GetField<Weapon.Stats>(weapon, "currentStats");
        PlayerMovement storedMovement = GetField<PlayerMovement>(weapon, "movement");

        Assert.AreSame(data, weapon.data);
        Assert.AreEqual(3f, currentStats.damage);
        Assert.AreSame(movement, storedMovement);
    }

    [Test]
    public void GetDamage_ShouldMultiplyStatsDamageByOwnerMight()
    {
        GameObject playerObject = new GameObject("Player");
        PlayerStats playerStats = playerObject.AddComponent<PlayerStats>();
        playerStats.enabled = false;

        CharacterData.Stats ownerStats = new CharacterData.Stats
        {
            might = 2f
        };
        playerStats.Stats = ownerStats;

        GameObject weaponObject = new GameObject("Weapon");
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();

        typeof(Item)
            .GetField("owner", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(weapon, playerStats);

        typeof(Weapon)
            .GetField("currentStats", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(weapon, new Weapon.Stats
            {
                damage = 4f,
                damageVariance = 0f
            });

        float result = weapon.GetDamage();

        Assert.AreEqual(8f, result);
    }

    [Test]
    public void GetStats_ShouldReturnCurrentStats()
    {
        GameObject weaponObject = new GameObject("Weapon");
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();

        Weapon.Stats stats = new Weapon.Stats
        {
            damage = 6f,
            cooldown = 2f,
            number = 3
        };

        typeof(Weapon)
            .GetField("currentStats", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(weapon, stats);

        Weapon.Stats result = weapon.GetStats();

        Assert.AreEqual(6f, result.damage);
        Assert.AreEqual(2f, result.cooldown);
        Assert.AreEqual(3, result.number);
    }
}