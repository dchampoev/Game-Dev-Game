using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ProjectileWeaponTests
{
    private class TestProjectileWeapon : ProjectileWeapon
    {
        public bool CallCanAttack() => CanAttack();
        public float CallGetSpawnAngle() => GetSpawnAngle();
        public Vector2 CallGetSpawnOffset(float angle = 0f) => GetSpawnOffset(angle);

        public void SetCurrentAttackCount(int value)
        {
            typeof(ProjectileWeapon)
                .GetField("currentAttackCount", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(this, value);
        }

        public void SetMovement(PlayerMovement value)
        {
            typeof(Weapon)
                .GetField("movement", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(this, value);
        }

        public void SetCurrentStats(Weapon.Stats value)
        {
            typeof(Weapon)
                .GetField("currentStats", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(this, value);
        }

        public void SetCurrentCooldown(float value)
        {
            typeof(Weapon)
                .GetField("currentCooldown", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(this, value);
        }
    }

    [Test]
    public void CanAttack_WhenCurrentAttackCountIsGreaterThanZero_ShouldReturnTrue()
    {
        GameObject weaponObject = new GameObject("Weapon");
        TestProjectileWeapon weapon = weaponObject.AddComponent<TestProjectileWeapon>();
        weapon.SetCurrentAttackCount(1);

        bool result = weapon.CallCanAttack();

        Assert.IsTrue(result);
    }

    [Test]
    public void GetSpawnAngle_WhenMovingRight_ShouldReturnZero()
    {
        GameObject playerObject = new GameObject("Player");
        PlayerMovement movement = playerObject.AddComponent<PlayerMovement>();
        movement.lastMoveDirection = Vector2.right;

        GameObject weaponObject = new GameObject("Weapon");
        TestProjectileWeapon weapon = weaponObject.AddComponent<TestProjectileWeapon>();
        weapon.SetMovement(movement);

        float result = weapon.CallGetSpawnAngle();

        Assert.That(Mathf.Abs(result), Is.LessThan(0.01f));
    }

    [Test]
    public void GetSpawnAngle_WhenMovingUp_ShouldReturnNinety()
    {
        GameObject playerObject = new GameObject("Player");
        PlayerMovement movement = playerObject.AddComponent<PlayerMovement>();
        movement.lastMoveDirection = Vector2.up;

        GameObject weaponObject = new GameObject("Weapon");
        TestProjectileWeapon weapon = weaponObject.AddComponent<TestProjectileWeapon>();
        weapon.SetMovement(movement);

        float result = weapon.CallGetSpawnAngle();

        Assert.That(Mathf.Abs(result - 90f), Is.LessThan(0.01f));
    }

    [Test]
    public void GetSpawnOffset_WhenVarianceIsFixedAndAngleIsNinety_ShouldRotateOffset()
    {
        GameObject weaponObject = new GameObject("Weapon");
        TestProjectileWeapon weapon = weaponObject.AddComponent<TestProjectileWeapon>();

        weapon.SetCurrentStats(new Weapon.Stats
        {
            spawnVariance = new Rect(1f, 0f, 0f, 0f)
        });

        Vector2 result = weapon.CallGetSpawnOffset(90f);

        Assert.That(Mathf.Abs(result.x), Is.LessThan(0.01f));
        Assert.That(Mathf.Abs(result.y - 1f), Is.LessThan(0.01f));
    }

    [Test]
    public void Attack_WhenProjectilePrefabIsMissing_ShouldReturnFalseAndSetBaseCooldown()
    {
        GameObject weaponObject = new GameObject("Weapon");
        TestProjectileWeapon weapon = weaponObject.AddComponent<TestProjectileWeapon>();

        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
        data.baseStats = new Weapon.Stats
        {
            cooldown = 2f
        };
        data.linearGrowth = new Weapon.Stats[0];
        data.randomGrowth = new Weapon.Stats[0];
        weapon.data = data;

        weapon.SetCurrentStats(new Weapon.Stats
        {
            projectilePrefab = null
        });

        MethodInfo attackMethod = typeof(ProjectileWeapon).GetMethod("Attack", BindingFlags.Instance | BindingFlags.NonPublic);

        LogAssert.Expect(LogType.Warning, $"Cannot attack with {weapon.name} because it has no projectile prefab assigned.");

        bool result = (bool)attackMethod.Invoke(weapon, new object[] { 1 });

        float cooldown = (float)typeof(Weapon)
            .GetField("currentCooldown", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(weapon);

        Assert.IsFalse(result);
        Assert.AreEqual(2f, cooldown);
    }
}