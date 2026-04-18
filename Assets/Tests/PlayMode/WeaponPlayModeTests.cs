using System.Reflection;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class WeaponPlayModeTests
{
    private readonly List<ScriptableObject> createdScriptableObjects = new List<ScriptableObject>();

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

    private WeaponData CreateWeaponData()
    {
        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
        createdScriptableObjects.Add(data);
        data.maxLevel = 5;
        data.baseStats = new Weapon.Stats
        {
            damage = 3f,
            cooldown = 1.5f
        };
        data.linearGrowth = new Weapon.Stats[0];
        data.randomGrowth = new Weapon.Stats[0];
        return data;
    }

    [TearDown]
    public void TearDown()
    {
        LogAssert.ignoreFailingMessages = false;

        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(go);
        }

        foreach (var obj in createdScriptableObjects)
        {
            if (obj != null)
            {
                Object.DestroyImmediate(obj, true);
            }
        }

        createdScriptableObjects.Clear();
    }

    [Test]
    public void Initialize_ShouldSetMovement_WhenPlayerExists()
    {
        LogAssert.ignoreFailingMessages = true;

        GameObject playerObject = new GameObject("Player");
        playerObject.tag = "Player";

        PlayerMovement movement = playerObject.AddComponent<PlayerMovement>();
        movement.enabled = false;

        PlayerStats playerStats = playerObject.AddComponent<PlayerStats>();
        playerStats.enabled = false;

        WeaponData data = CreateWeaponData();

        GameObject weaponObject = new GameObject("Weapon");
        weaponObject.SetActive(false);
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();

        weapon.Initialize(data);

        PlayerMovement storedMovement = GetField<PlayerMovement>(weapon, "movement");

        Assert.AreSame(movement, storedMovement);
    }
}