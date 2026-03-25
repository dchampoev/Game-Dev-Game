using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ProjectileWeaponPlayModeTests
{
    private class TestProjectile : Projectile
    {
        protected override void Start()
        {
        }

        protected override void FixedUpdate()
        {
        }
    }

    private class TestProjectileWeapon : ProjectileWeapon
    {
        public bool CallAttack(int attackCount = 1)
        {
            return (bool)typeof(ProjectileWeapon)
                .GetMethod("Attack", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(this, new object[] { attackCount });
        }

        public void CallUpdate()
        {
            typeof(ProjectileWeapon)
                .GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(this, null);
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

        public float GetCurrentCooldown()
        {
            return (float)typeof(Weapon)
                .GetField("currentCooldown", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(this);
        }

        public int GetCurrentAttackCount()
        {
            return (int)typeof(ProjectileWeapon)
                .GetField("currentAttackCount", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(this);
        }

        public float GetCurrentAttackInterval()
        {
            return (float)typeof(ProjectileWeapon)
                .GetField("currentAttackInterval", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(this);
        }

        public void SetCurrentAttackInterval(float value)
        {
            typeof(ProjectileWeapon)
                .GetField("currentAttackInterval", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(this, value);
        }

        public void SetCurrentAttackCount(int value)
        {
            typeof(ProjectileWeapon)
                .GetField("currentAttackCount", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(this, value);
        }

        public void SetOwner(PlayerStats value)
        {
            typeof(Item)
                .GetField("owner", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(this, value);
        }

        public void SetMovement(PlayerMovement value)
        {
            typeof(Weapon)
                .GetField("movement", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(this, value);
        }
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.Destroy(go);
        }

        yield return null;
    }

    [UnityTest]
    public IEnumerator Attack_WhenProjectilePrefabExists_ShouldSpawnProjectileAndSetCooldown()
    {
        GameObject playerObject = new GameObject("Player");
        PlayerStats owner = playerObject.AddComponent<PlayerStats>();
        owner.enabled = false;
        PlayerMovement movement = playerObject.AddComponent<PlayerMovement>();
        movement.enabled = false;
        movement.lastMoveDirection = Vector2.right;

        GameObject projectilePrefabObject = new GameObject("ProjectilePrefab");
        TestProjectile projectilePrefab = projectilePrefabObject.AddComponent<TestProjectile>();

        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
        data.baseStats = new Weapon.Stats
        {
            cooldown = 2f,
            projectileInterval = 0.5f
        };
        data.linearGrowth = new Weapon.Stats[0];
        data.randomGrowth = new Weapon.Stats[0];

        GameObject weaponObject = new GameObject("Weapon");
        TestProjectileWeapon weapon = weaponObject.AddComponent<TestProjectileWeapon>();
        weapon.data = data;
        weapon.SetOwner(owner);
        weapon.SetMovement(movement);
        weapon.SetCurrentCooldown(0f);
        weapon.SetCurrentStats(new Weapon.Stats
        {
            projectilePrefab = projectilePrefab,
            cooldown = 2f,
            spawnVariance = new Rect(0f, 0f, 0f, 0f)
        });

        bool result = weapon.CallAttack(1);

        TestProjectile[] projectiles = Object.FindObjectsByType<TestProjectile>(FindObjectsSortMode.None);

        Assert.IsTrue(result);
        Assert.AreEqual(2, projectiles.Length);
        Assert.AreEqual(2f, weapon.GetCurrentCooldown());

        yield return null;
    }

    [UnityTest]
    public IEnumerator Attack_WhenAttackCountIsGreaterThanOne_ShouldQueueNextAttack()
    {
        GameObject playerObject = new GameObject("Player");
        PlayerStats owner = playerObject.AddComponent<PlayerStats>();
        owner.enabled = false;
        PlayerMovement movement = playerObject.AddComponent<PlayerMovement>();
        movement.enabled = false;
        movement.lastMoveDirection = Vector2.right;

        GameObject projectilePrefabObject = new GameObject("ProjectilePrefab");
        TestProjectile projectilePrefab = projectilePrefabObject.AddComponent<TestProjectile>();

        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
        data.baseStats = new Weapon.Stats
        {
            cooldown = 2f,
            projectileInterval = 0.75f
        };
        data.linearGrowth = new Weapon.Stats[0];
        data.randomGrowth = new Weapon.Stats[0];

        GameObject weaponObject = new GameObject("Weapon");
        TestProjectileWeapon weapon = weaponObject.AddComponent<TestProjectileWeapon>();
        weapon.data = data;
        weapon.SetOwner(owner);
        weapon.SetMovement(movement);
        weapon.SetCurrentCooldown(0f);
        weapon.SetCurrentStats(new Weapon.Stats
        {
            projectilePrefab = projectilePrefab,
            cooldown = 2f,
            spawnVariance = new Rect(0f, 0f, 0f, 0f)
        });

        bool result = weapon.CallAttack(3);

        Assert.IsTrue(result);
        Assert.AreEqual(2, weapon.GetCurrentAttackCount());
        Assert.AreEqual(0.75f, weapon.GetCurrentAttackInterval());

        yield return null;
    }

    [UnityTest]
    public IEnumerator Update_WhenQueuedAttackIntervalExpires_ShouldFireNextProjectile()
    {
        GameObject playerObject = new GameObject("Player");
        PlayerStats owner = playerObject.AddComponent<PlayerStats>();
        owner.enabled = false;
        PlayerMovement movement = playerObject.AddComponent<PlayerMovement>();
        movement.enabled = false;
        movement.lastMoveDirection = Vector2.right;

        GameObject projectilePrefabObject = new GameObject("ProjectilePrefab");
        TestProjectile projectilePrefab = projectilePrefabObject.AddComponent<TestProjectile>();

        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
        data.baseStats = new Weapon.Stats
        {
            cooldown = 2f,
            projectileInterval = 0.01f
        };
        data.linearGrowth = new Weapon.Stats[0];
        data.randomGrowth = new Weapon.Stats[0];

        GameObject weaponObject = new GameObject("Weapon");
        TestProjectileWeapon weapon = weaponObject.AddComponent<TestProjectileWeapon>();
        weapon.data = data;
        weapon.SetOwner(owner);
        weapon.SetMovement(movement);
        weapon.SetCurrentCooldown(0f);
        weapon.SetCurrentAttackCount(1);
        weapon.SetCurrentAttackInterval(0f);
        weapon.SetCurrentStats(new Weapon.Stats
        {
            projectilePrefab = projectilePrefab,
            cooldown = 2f,
            spawnVariance = new Rect(0f, 0f, 0f, 0f)
        });

        weapon.CallUpdate();

        TestProjectile[] projectiles = Object.FindObjectsByType<TestProjectile>(FindObjectsSortMode.None);

        Assert.AreEqual(2, projectiles.Length);

        yield return null;
    }
}