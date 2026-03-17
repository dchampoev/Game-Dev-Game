using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using System.Reflection;

public class ProjectileWeaponBehaviourTests
{
    private class TestProjectileWeaponBehaviour : ProjectileWeaponBehaviour
    {
        public void CallStart()
        {
            base.Start();
        }

        public void CallOnTriggerEnter2D(Collider2D collider)
        {
            base.OnTriggerEnter2D(collider);
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

    private void CallPrivateAwake(ProjectileWeaponBehaviour behaviour)
    {
        typeof(ProjectileWeaponBehaviour)
            .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(behaviour, null);
    }

    private float GetPrivateFloat(ProjectileWeaponBehaviour behaviour, string fieldName)
    {
        return (float)typeof(ProjectileWeaponBehaviour)
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(behaviour);
    }

    private int GetPrivateInt(ProjectileWeaponBehaviour behaviour, string fieldName)
    {
        return (int)typeof(ProjectileWeaponBehaviour)
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(behaviour);
    }

    [Test]
    public void Awake_ShouldInitializeStatsFromWeaponData()
    {
        GameObject projectileObject = new GameObject("Projectile");
        TestProjectileWeaponBehaviour behaviour = projectileObject.AddComponent<TestProjectileWeaponBehaviour>();
        behaviour.weaponData = CreateWeaponData(damage: 3f, speed: 7f, cooldown: 2f, pierce: 4);

        CallPrivateAwake(behaviour);

        Assert.AreEqual(3f, GetPrivateFloat(behaviour, "currentDamage"));
        Assert.AreEqual(7f, GetPrivateFloat(behaviour, "currentSpeed"));
        Assert.AreEqual(2f, GetPrivateFloat(behaviour, "currentCooldownDuration"));
        Assert.AreEqual(4, GetPrivateInt(behaviour, "currentPierce"));

        Object.DestroyImmediate(behaviour.weaponData);
        Object.DestroyImmediate(projectileObject);
    }

    [Test]
    public void DirectionChecker_ShouldRotateObjectTowardsDirection()
    {
        GameObject projectileObject = new GameObject("Projectile");
        TestProjectileWeaponBehaviour behaviour = projectileObject.AddComponent<TestProjectileWeaponBehaviour>();

        behaviour.DirectionChecker(Vector3.right);

        float zRotation = projectileObject.transform.rotation.eulerAngles.z;

        Assert.AreEqual(315f, zRotation, 0.1f);

        Object.DestroyImmediate(projectileObject);
    }

    [Test]
    public void GetCurrentDamage_ShouldMultiplyWeaponDamageByPlayerMight()
    {
        GameObject playerObject = new GameObject("Player");
        PlayerStats playerStats = playerObject.AddComponent<PlayerStats>();
        playerStats.CurrentMight = 2f;

        GameObject projectileObject = new GameObject("Projectile");
        TestProjectileWeaponBehaviour behaviour = projectileObject.AddComponent<TestProjectileWeaponBehaviour>();
        behaviour.weaponData = CreateWeaponData(damage: 3f, speed: 1f, cooldown: 1f, pierce: 2);

        CallPrivateAwake(behaviour);

        float result = behaviour.GetCurrentDamage();

        Assert.AreEqual(6f, result);

        Object.DestroyImmediate(behaviour.weaponData);
        Object.DestroyImmediate(projectileObject);
        Object.DestroyImmediate(playerObject);
    }

    [Test]
    public void OnTriggerEnter2D_WhenWeaponHitsProp_ShouldDamagePropAndReducePierce()
    {
        GameObject playerObject = new GameObject("Player");
        PlayerStats playerStats = playerObject.AddComponent<PlayerStats>();
        playerStats.CurrentMight = 1f;

        GameObject projectileObject = new GameObject("Projectile");
        TestProjectileWeaponBehaviour behaviour = projectileObject.AddComponent<TestProjectileWeaponBehaviour>();
        behaviour.weaponData = CreateWeaponData(damage: 2f, speed: 1f, cooldown: 1f, pierce: 2);

        CallPrivateAwake(behaviour);

        GameObject propObject = new GameObject("Prop");
        propObject.tag = "Prop";
        BoxCollider2D collider = propObject.AddComponent<BoxCollider2D>();
        BreakableProps breakableProps = propObject.AddComponent<BreakableProps>();
        breakableProps.health = 10f;

        behaviour.CallOnTriggerEnter2D(collider);

        Assert.AreEqual(8f, breakableProps.health);
        Assert.AreEqual(1, GetPrivateInt(behaviour, "currentPierce"));

        Object.DestroyImmediate(behaviour.weaponData);
        Object.DestroyImmediate(propObject);
        Object.DestroyImmediate(projectileObject);
        Object.DestroyImmediate(playerObject);
    }
    [Test]
    public void OnTriggerEnter2D_WhenWeaponHitsProp_ShouldReducePropHealth()
    {
        GameObject projectile = new GameObject();
        var behaviour = projectile.AddComponent<ProjectileWeaponBehaviour>();

        behaviour.weaponData = ScriptableObject.CreateInstance<WeaponScriptableObject>();

        typeof(ProjectileWeaponBehaviour)
            .GetField("currentDamage", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(behaviour, 2f);

        typeof(ProjectileWeaponBehaviour)
            .GetField("currentPierce", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(behaviour, 2);

        GameObject prop = new GameObject();
        prop.tag = "Prop";

        var collider = prop.AddComponent<BoxCollider2D>();
        var breakable = prop.AddComponent<BreakableProps>();
        breakable.health = 10f;

        typeof(ProjectileWeaponBehaviour)
            .GetMethod("OnTriggerEnter2D", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(behaviour, new object[] { collider });

        Assert.AreEqual(8f, breakable.health);

        Object.DestroyImmediate(projectile);
        Object.DestroyImmediate(prop);
    }
    [Test]
    public void DirectionChecker_ShouldRotateCorrectly()
    {
        GameObject obj = new GameObject();
        var behaviour = obj.AddComponent<ProjectileWeaponBehaviour>();

        behaviour.DirectionChecker(Vector3.right);

        float z = obj.transform.rotation.eulerAngles.z;

        Assert.AreEqual(315f, z, 0.1f);

        Object.DestroyImmediate(obj);
    }
}