using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class WeaponPlayModeTests
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

    [UnityTest]
    public IEnumerator Initialize_ShouldSetMovement_WhenPlayerExists()
    {
        GameObject playerObject = new GameObject("Player");

        PlayerStats playerStats = playerObject.AddComponent<PlayerStats>();
        playerStats.enabled = false;

        PlayerMovement movement = playerObject.AddComponent<PlayerMovement>();
        movement.enabled = false;

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

        PlayerMovement storedMovement = GetField<PlayerMovement>(weapon, "movement");

        Assert.AreSame(movement, storedMovement);

        yield return null;
    }
}