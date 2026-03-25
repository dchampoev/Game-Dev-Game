using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class WhipWeaponPlayModeTests
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
    public IEnumerator Attack_WhenMovingRight_ShouldSpawnProjectileOnPositiveX()
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
            cooldown = 1f,
            projectileInterval = 0.25f
        };
        data.linearGrowth = new Weapon.Stats[0];
        data.randomGrowth = new Weapon.Stats[0];

        GameObject weaponObject = new GameObject("Whip");
        TestWhipWeapon weapon = weaponObject.AddComponent<TestWhipWeapon>();
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

        yield return null;
    }

    [UnityTest]
    public IEnumerator Attack_WhenSecondSpawnGoesLeft_ShouldFlipProjectileScale()
    {
        GameObject playerObject = new GameObject("Player");
        PlayerStats owner = playerObject.AddComponent<PlayerStats>();
        owner.enabled = false;

        PlayerMovement movement = playerObject.AddComponent<PlayerMovement>();
        movement.enabled = false;
        movement.lastMoveDirection = Vector2.right;

        GameObject projectilePrefabObject = new GameObject("ProjectilePrefab");
        TestProjectile projectilePrefab = projectilePrefabObject.AddComponent<TestProjectile>();
        projectilePrefab.transform.localScale = new Vector3(2f, 1f, 1f);

        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
        data.baseStats = new Weapon.Stats
        {
            cooldown = 1f,
            projectileInterval = 0.25f
        };
        data.linearGrowth = new Weapon.Stats[0];
        data.randomGrowth = new Weapon.Stats[0];

        GameObject weaponObject = new GameObject("Whip");
        TestWhipWeapon weapon = weaponObject.AddComponent<TestWhipWeapon>();
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
            cooldown = 1f,
            projectileInterval = 0.5f
        };
        data.linearGrowth = new Weapon.Stats[0];
        data.randomGrowth = new Weapon.Stats[0];

        GameObject weaponObject = new GameObject("Whip");
        TestWhipWeapon weapon = weaponObject.AddComponent<TestWhipWeapon>();
        weapon.data = data;
        weapon.SetOwner(owner);
        weapon.SetMovement(movement);
        weapon.SetCurrentCooldown(0f);
        weapon.SetCurrentStats(new Weapon.Stats
        {
            projectilePrefab = projectilePrefab,
            spawnVariance = new Rect(1f, 0f, 0f, 0f)
        });

        bool result = weapon.CallAttack(3);

        Assert.IsTrue(result);
        Assert.AreEqual(2, weapon.GetCurrentAttackCount());
        Assert.AreEqual(0.5f, weapon.GetCurrentAttackInterval());

        yield return null;
    }
}