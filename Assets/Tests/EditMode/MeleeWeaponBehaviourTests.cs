using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using System.Reflection;

public class MeleeWeaponBehaviourTests
{
    private class TestMeleeWeaponBehaviour : MeleeWeaponBehaviour
    {
        public void CallOnTriggerEnter2D(Collider2D collider)
        {
            typeof(MeleeWeaponBehaviour)
                .GetMethod("OnTriggerEnter2D", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.Invoke(this, new object[] { collider });
        }
    }

    private WeaponScriptableObject CreateWeaponData(
        float damage = 2f,
        float speed = 5f,
        float cooldown = 1f,
        int pierce = 2)
    {
        WeaponScriptableObject data = ScriptableObject.CreateInstance<WeaponScriptableObject>();
        SerializedObject so = new SerializedObject(data);
        so.FindProperty("damage").floatValue = damage;
        so.FindProperty("speed").floatValue = speed;
        so.FindProperty("cooldownDuration").floatValue = cooldown;
        so.FindProperty("pierce").intValue = pierce;
        so.ApplyModifiedProperties();
        return data;
    }
    private float GetPrivateFloat(MeleeWeaponBehaviour behaviour, string fieldName)
    {
        return (float)typeof(MeleeWeaponBehaviour)
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(behaviour);
    }

    private void SetPrivateFloat(MeleeWeaponBehaviour behaviour, string fieldName, float value)
    {
        typeof(MeleeWeaponBehaviour)
            .GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            .SetValue(behaviour, value);
    }

    private void SetPrivateInt(MeleeWeaponBehaviour behaviour, string fieldName, int value)
    {
        typeof(MeleeWeaponBehaviour)
            .GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            .SetValue(behaviour, value);
    }

    private void InitializeStats(MeleeWeaponBehaviour behaviour)
    {
        typeof(MeleeWeaponBehaviour)
            .GetMethod("InitializeStats", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
            ?.Invoke(behaviour, null);
    }
    [TearDown]
    public void TearDown()
    {
        foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(obj);
        }
    }

    [Test]
    public void Awake_ShouldInitializeStatsFromWeaponData()
    {
        GameObject weaponObject = new GameObject("MeleeWeapon");
        TestMeleeWeaponBehaviour behaviour = weaponObject.AddComponent<TestMeleeWeaponBehaviour>();
        behaviour.weaponData = CreateWeaponData(damage: 3f, speed: 7f, cooldown: 2f, pierce: 4);

        InitializeStats(behaviour);

        Assert.AreEqual(3f, GetPrivateFloat(behaviour, "currentDamage"));
        Assert.AreEqual(7f, GetPrivateFloat(behaviour, "currentSpeed"));
        Assert.AreEqual(2f, GetPrivateFloat(behaviour, "currentCooldownDuration"));
        Assert.AreEqual(4f, (float)(int)typeof(MeleeWeaponBehaviour)
            .GetField("currentPierce", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(behaviour));

        Object.DestroyImmediate(behaviour.weaponData);
        Object.DestroyImmediate(weaponObject);
    }

    [Test]
    public void GetCurrentDamage_WhenPlayerExists_ShouldMultiplyDamageByPlayerMight()
    {
        GameObject playerObject = new GameObject("Player");
        PlayerStats playerStats = playerObject.AddComponent<PlayerStats>();
        playerStats.CurrentMight = 2f;

        GameObject weaponObject = new GameObject("MeleeWeapon");
        TestMeleeWeaponBehaviour behaviour = weaponObject.AddComponent<TestMeleeWeaponBehaviour>();

        SetPrivateFloat(behaviour, "currentDamage", 3f);

        float result = behaviour.GetCurrentDamage();

        Assert.AreEqual(6f, result);

        Object.DestroyImmediate(weaponObject);
        Object.DestroyImmediate(playerObject);
    }

    [Test]
    public void GetCurrentDamage_WhenPlayerDoesNotExist_ShouldUseDefaultMightOne()
    {
        GameObject weaponObject = new GameObject("MeleeWeapon");
        TestMeleeWeaponBehaviour behaviour = weaponObject.AddComponent<TestMeleeWeaponBehaviour>();

        SetPrivateFloat(behaviour, "currentDamage", 3f);

        float result = behaviour.GetCurrentDamage();

        Assert.AreEqual(3f, result);

        Object.DestroyImmediate(weaponObject);
    }

    [Test]
    public void OnTriggerEnter2D_WhenWeaponHitsEnemy_ShouldReduceEnemyHealth()
    {
        GameObject weaponObject = new GameObject("MeleeWeapon");
        TestMeleeWeaponBehaviour behaviour = weaponObject.AddComponent<TestMeleeWeaponBehaviour>();

        SetPrivateFloat(behaviour, "currentDamage", 2f);
        SetPrivateInt(behaviour, "currentPierce", 2);

        GameObject enemyObject = new GameObject("Enemy");
        enemyObject.tag = "Enemy";
        BoxCollider2D collider = enemyObject.AddComponent<BoxCollider2D>();

        EnemyStats enemyStats = enemyObject.AddComponent<EnemyStats>();
        enemyStats.enabled = false;
        enemyStats.currentHealth = 10f;

        behaviour.CallOnTriggerEnter2D(collider);

        Assert.AreEqual(8f, enemyStats.currentHealth);

        Object.DestroyImmediate(enemyObject);
        Object.DestroyImmediate(weaponObject);
    }

    [Test]
    public void OnTriggerEnter2D_WhenWeaponHitsProp_ShouldReducePropHealth()
    {
        GameObject weaponObject = new GameObject("MeleeWeapon");
        TestMeleeWeaponBehaviour behaviour = weaponObject.AddComponent<TestMeleeWeaponBehaviour>();

        SetPrivateFloat(behaviour, "currentDamage", 2f);
        SetPrivateInt(behaviour, "currentPierce", 2);

        GameObject propObject = new GameObject("Prop");
        propObject.tag = "Prop";
        BoxCollider2D collider = propObject.AddComponent<BoxCollider2D>();
        BreakableProps breakableProps = propObject.AddComponent<BreakableProps>();
        breakableProps.health = 10f;

        behaviour.CallOnTriggerEnter2D(collider);

        Assert.AreEqual(8f, breakableProps.health);

        Object.DestroyImmediate(propObject);
        Object.DestroyImmediate(weaponObject);
    }
}