using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class WhipWeaponPlayModeTests
{
    private readonly List<ScriptableObject> createdScriptableObjects = new List<ScriptableObject>();

    private class TestProjectile : Projectile
    {
        protected override void Start()
        {
        }

        protected override void FixedUpdate()
        {
        }
    }

    private class TestWhipWeapon : WhipWeapon
    {
        public bool CallAttack(int attackCount = 1)
        {
            return (bool)typeof(WhipWeapon)
                .GetMethod("Attack", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(this, new object[] { attackCount });
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

        public int GetCurrentSpawnCount()
        {
            return (int)typeof(WhipWeapon)
                .GetField("currentSpawnCount", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(this);
        }

        public float GetCurrentSpawnYOffset()
        {
            return (float)typeof(WhipWeapon)
                .GetField("currentSpawnYOffset", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(this);
        }

        public void SetCurrentSpawnCount(int value)
        {
            typeof(WhipWeapon)
                .GetField("currentSpawnCount", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(this, value);
        }

        public void SetCurrentSpawnYOffset(float value)
        {
            typeof(WhipWeapon)
                .GetField("currentSpawnYOffset", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(this, value);
        }

        public void SetCurrentAttackCount(int value)
        {
            typeof(ProjectileWeapon)
                .GetField("currentAttackCount", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(this, value);
        }
    }

    private PlayerStats CreateInactiveOwner(out PlayerMovement movement)
    {
        GameObject playerObject = new GameObject("Player");
        playerObject.SetActive(false);

        PlayerStats owner = playerObject.AddComponent<PlayerStats>();
        owner.enabled = false;

        movement = playerObject.AddComponent<PlayerMovement>();
        movement.enabled = false;
        movement.lastMoveDirection = Vector2.right;

        return owner;
    }

    private WeaponData CreateWeaponData(float cooldown, float projectileInterval)
    {
        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
        createdScriptableObjects.Add(data);
        data.baseStats = new Weapon.Stats
        {
            cooldown = cooldown,
            projectileInterval = projectileInterval
        };
        data.linearGrowth = new Weapon.Stats[0];
        data.randomGrowth = new Weapon.Stats[0];
        return data;
    }

    private TestWhipWeapon CreateInactiveWeapon()
    {
        GameObject weaponObject = new GameObject("Whip");
        weaponObject.SetActive(false);
        return weaponObject.AddComponent<TestWhipWeapon>();
    }

    [TearDown]
    public void TearDown()
    {
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
    public void Attack_WhenMovingRight_ShouldSpawnProjectileOnPositiveX()
    {
        PlayerMovement movement;
        PlayerStats owner = CreateInactiveOwner(out movement);

        GameObject projectilePrefabObject = new GameObject("ProjectilePrefab");
        TestProjectile projectilePrefab = projectilePrefabObject.AddComponent<TestProjectile>();

        WeaponData data = CreateWeaponData(1f, 0.25f);

        TestWhipWeapon weapon = CreateInactiveWeapon();
        weapon.data = data;
        weapon.SetOwner(owner);
        weapon.SetMovement(movement);
        weapon.SetCurrentCooldown(0f);
        weapon.SetCurrentStats(new Weapon.Stats
        {
            projectilePrefab = projectilePrefab,
            spawnVariance = new Rect(1f, 0f, 0f, 0f)
        });

        bool result = weapon.CallAttack(1);

        TestProjectile[] projectiles = Object.FindObjectsByType<TestProjectile>(FindObjectsSortMode.None);
        TestProjectile spawned = null;

        foreach (var p in projectiles)
        {
            if (p != projectilePrefab)
                spawned = p;
        }

        Assert.IsTrue(result);
        Assert.NotNull(spawned);
        Assert.Greater(spawned.transform.position.x, owner.transform.position.x);
        Assert.AreEqual(1, weapon.GetCurrentSpawnCount());
        Assert.AreEqual(0f, weapon.GetCurrentSpawnYOffset());
    }

    [Test]
    public void Attack_WhenSecondSpawnGoesLeft_ShouldFlipProjectileScale()
    {
        PlayerMovement movement;
        PlayerStats owner = CreateInactiveOwner(out movement);

        GameObject projectilePrefabObject = new GameObject("ProjectilePrefab");
        TestProjectile projectilePrefab = projectilePrefabObject.AddComponent<TestProjectile>();
        projectilePrefab.transform.localScale = new Vector3(2f, 1f, 1f);

        WeaponData data = CreateWeaponData(1f, 0.25f);

        TestWhipWeapon weapon = CreateInactiveWeapon();
        weapon.data = data;
        weapon.SetOwner(owner);
        weapon.SetMovement(movement);
        weapon.SetCurrentCooldown(1f);
        weapon.SetCurrentAttackCount(1);
        weapon.SetCurrentSpawnCount(1);
        weapon.SetCurrentSpawnYOffset(0f);
        weapon.SetCurrentStats(new Weapon.Stats
        {
            projectilePrefab = projectilePrefab,
            spawnVariance = new Rect(1f, 0f, 0f, 0f)
        });

        bool result = weapon.CallAttack(1);

        TestProjectile[] projectiles = Object.FindObjectsByType<TestProjectile>(FindObjectsSortMode.None);
        TestProjectile spawned = null;

        foreach (var p in projectiles)
        {
            if (p != projectilePrefab)
                spawned = p;
        }

        Assert.IsTrue(result);
        Assert.NotNull(spawned);
        Assert.Less(spawned.transform.position.x, owner.transform.position.x);
        Assert.Less(spawned.transform.localScale.x, 0f);
        Assert.AreEqual(2, weapon.GetCurrentSpawnCount());
        Assert.AreEqual(1f, weapon.GetCurrentSpawnYOffset());
    }

    [Test]
    public void Attack_WhenAttackCountIsGreaterThanOne_ShouldQueueNextAttack()
    {
        PlayerMovement movement;
        PlayerStats owner = CreateInactiveOwner(out movement);

        GameObject projectilePrefabObject = new GameObject("ProjectilePrefab");
        TestProjectile projectilePrefab = projectilePrefabObject.AddComponent<TestProjectile>();

        WeaponData data = CreateWeaponData(1f, 0.5f);

        TestWhipWeapon weapon = CreateInactiveWeapon();
        weapon.data = data;
        weapon.SetOwner(owner);
        weapon.SetMovement(movement);
        weapon.SetCurrentCooldown(0f);
        weapon.SetCurrentStats(new Weapon.Stats
        {
            projectilePrefab = projectilePrefab,
            projectileInterval = 0.5f,
            spawnVariance = new Rect(1f, 0f, 0f, 0f)
        });

        bool result = weapon.CallAttack(3);

        Assert.IsTrue(result);
        Assert.AreEqual(2, weapon.GetCurrentAttackCount());
        Assert.AreEqual(0.5f, weapon.GetCurrentAttackInterval());
    }
}