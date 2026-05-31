using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class KingBibleWeaponTests
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

    private class TestKingBibleWeapon : KingBibleWeapon
    {
        public bool CallAttack(int attackCount = 1)
        {
            return (bool)typeof(KingBibleWeapon)
                .GetMethod("Attack", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(this, new object[] { attackCount });
        }

        public void SetOwner(PlayerStats value)
        {
            typeof(Item)
                .GetField("owner", BindingFlags.Instance | BindingFlags.NonPublic)
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

        public Weapon.Stats GetCurrentStats()
        {
            return (Weapon.Stats)typeof(Weapon)
                .GetField("currentStats", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(this);
        }
    }

    private PlayerStats CreateOwner()
    {
        GameObject playerObject = new GameObject("Player");
        playerObject.SetActive(false);

        PlayerStats owner = playerObject.AddComponent<PlayerStats>();
        owner.enabled = false;
        owner.Stats = new CharacterData.Stats
        {
            might = 1f,
            area = 1f,
            speed = 1f,
            duration = 1f,
            amount = 1,
            cooldown = 0.5f
        };

        return owner;
    }

    private TestProjectile CreateProjectilePrefab()
    {
        GameObject projectileObject = new GameObject("BibleProjectilePrefab");
        projectileObject.SetActive(false);
        return projectileObject.AddComponent<TestProjectile>();
    }

    private WeaponData CreateWeaponData(float projectileInterval = 0.25f)
    {
        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
        data.maxLevel = 8;
        data.baseStats = new Weapon.Stats
        {
            projectileInterval = projectileInterval
        };
        data.linearGrowth = new[]
        {
            new Weapon.Stats
            {
                damage = 5f,
                number = 1
            }
        };
        data.randomGrowth = new Weapon.Stats[0];
        return data;
    }

    private int CountSpawnedProjectiles(TestProjectile projectilePrefab)
    {
        TestProjectile[] projectiles = Object.FindObjectsByType<TestProjectile>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );
        int spawnedCount = 0;

        foreach (TestProjectile projectile in projectiles)
        {
            if (projectile != projectilePrefab)
            {
                spawnedCount++;
            }
        }

        return spawnedCount;
    }

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject obj in Object.FindObjectsByType<GameObject>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        ))
        {
            Object.DestroyImmediate(obj);
        }

        TestScriptableObjectCleanup.DestroyRuntimeObjects<WeaponData>();
    }

    [Test]
    public void SpawnRing_ShouldCreateOneProjectilePerWeaponNumberAndOwnerAmount()
    {
        PlayerStats owner = CreateOwner();
        TestProjectile projectilePrefab = CreateProjectilePrefab();

        GameObject weaponObject = new GameObject("KingBible");
        TestKingBibleWeapon weapon = weaponObject.AddComponent<TestKingBibleWeapon>();
        weapon.SetOwner(owner);
        weapon.SetCurrentStats(new Weapon.Stats
        {
            projectilePrefab = projectilePrefab,
            area = 2f,
            number = 3
        });

        weapon.SpawnRing(owner);

        TestProjectile[] projectiles = Object.FindObjectsByType<TestProjectile>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );
        int spawnedCount = 0;

        foreach (TestProjectile projectile in projectiles)
        {
            if (projectile == projectilePrefab)
                continue;

            spawnedCount++;
            Assert.AreSame(owner, projectile.owner);
            Assert.AreSame(weapon, projectile.weapon);
            Assert.AreSame(owner.transform, projectile.transform.parent);
            Assert.That(Vector2.Distance(owner.transform.position, projectile.transform.position), Is.EqualTo(3f).Within(0.01f));
        }

        Assert.AreEqual(4, spawnedCount);
    }

    [Test]
    public void ActivateCooldown_ShouldUseLifespanPlusCooldownScaledByOwnerCooldown()
    {
        PlayerStats owner = CreateOwner();

        GameObject weaponObject = new GameObject("KingBible");
        TestKingBibleWeapon weapon = weaponObject.AddComponent<TestKingBibleWeapon>();
        weapon.SetOwner(owner);
        weapon.SetCurrentCooldown(0f);
        weapon.SetCurrentStats(new Weapon.Stats
        {
            lifespan = 3f,
            cooldown = 5f
        });

        bool result = weapon.ActivateCooldown(true);

        Assert.IsTrue(result);
        Assert.AreEqual(4f, weapon.GetCurrentCooldown());
    }

    [Test]
    public void ActivateCooldown_WhenStrictAndCooldownActive_ShouldNotChangeCooldown()
    {
        PlayerStats owner = CreateOwner();

        GameObject weaponObject = new GameObject("KingBible");
        TestKingBibleWeapon weapon = weaponObject.AddComponent<TestKingBibleWeapon>();
        weapon.SetOwner(owner);
        weapon.SetCurrentCooldown(1.5f);
        weapon.SetCurrentStats(new Weapon.Stats
        {
            lifespan = 3f,
            cooldown = 5f
        });

        bool result = weapon.ActivateCooldown(true);

        Assert.IsFalse(result);
        Assert.AreEqual(1.5f, weapon.GetCurrentCooldown());
    }

    [Test]
    public void Attack_WhenSuccessful_ShouldSpawnRingAndQueueRemainingAttacks()
    {
        PlayerStats owner = CreateOwner();
        TestProjectile projectilePrefab = CreateProjectilePrefab();

        GameObject weaponObject = new GameObject("KingBible");
        TestKingBibleWeapon weapon = weaponObject.AddComponent<TestKingBibleWeapon>();
        weapon.data = CreateWeaponData(0.15f);
        weapon.SetOwner(owner);
        weapon.SetCurrentCooldown(0f);
        weapon.SetCurrentStats(new Weapon.Stats
        {
            projectilePrefab = projectilePrefab,
            area = 1f,
            lifespan = 2f,
            cooldown = 4f,
            number = 2
        });

        bool result = weapon.CallAttack(3);

        Assert.IsTrue(result);
        Assert.AreEqual(3, CountSpawnedProjectiles(projectilePrefab));
        Assert.AreEqual(3f, weapon.GetCurrentCooldown());
        Assert.AreEqual(2, weapon.GetCurrentAttackCount());
        Assert.AreEqual(0.15f, weapon.GetCurrentAttackInterval());
    }

    [Test]
    public void Attack_WhenCurrentCooldownActive_ShouldNotSpawnRing()
    {
        PlayerStats owner = CreateOwner();
        TestProjectile projectilePrefab = CreateProjectilePrefab();

        GameObject weaponObject = new GameObject("KingBible");
        TestKingBibleWeapon weapon = weaponObject.AddComponent<TestKingBibleWeapon>();
        weapon.data = CreateWeaponData();
        weapon.SetOwner(owner);
        weapon.SetCurrentCooldown(1f);
        weapon.SetCurrentStats(new Weapon.Stats
        {
            projectilePrefab = projectilePrefab,
            area = 1f,
            lifespan = 2f,
            cooldown = 4f,
            number = 2
        });

        bool result = weapon.CallAttack();

        Assert.IsFalse(result);
        Assert.AreEqual(0, CountSpawnedProjectiles(projectilePrefab));
        Assert.AreEqual(1f, weapon.GetCurrentCooldown());
    }

    [Test]
    public void Attack_WhenProjectilePrefabMissing_ShouldWarnAndStartCooldown()
    {
        PlayerStats owner = CreateOwner();

        GameObject weaponObject = new GameObject("KingBible");
        TestKingBibleWeapon weapon = weaponObject.AddComponent<TestKingBibleWeapon>();
        weapon.name = "KingBible";
        weapon.SetOwner(owner);
        weapon.SetCurrentCooldown(0f);
        weapon.SetCurrentStats(new Weapon.Stats
        {
            projectilePrefab = null,
            lifespan = 2f,
            cooldown = 4f
        });

        LogAssert.Expect(LogType.Warning, "Projectile prefab has not been set for KingBible");

        bool result = weapon.CallAttack();

        Assert.IsFalse(result);
        Assert.AreEqual(3f, weapon.GetCurrentCooldown());
    }

    [Test]
    public void DoLevelUp_ShouldApplyGrowthSpawnNewRingAndRefreshCooldown()
    {
        PlayerStats owner = CreateOwner();
        TestProjectile projectilePrefab = CreateProjectilePrefab();

        WeaponData data = CreateWeaponData();
        data.baseStats = new Weapon.Stats
        {
            projectilePrefab = projectilePrefab,
            area = 1f,
            lifespan = 2f,
            cooldown = 4f,
            number = 1
        };

        GameObject weaponObject = new GameObject("KingBible");
        TestKingBibleWeapon weapon = weaponObject.AddComponent<TestKingBibleWeapon>();
        weapon.data = data;
        weapon.maxLevel = data.maxLevel;
        weapon.SetOwner(owner);
        weapon.SetCurrentCooldown(0f);
        weapon.SetCurrentStats(data.baseStats);

        bool result = weapon.DoLevelUp();

        Assert.IsTrue(result);
        Assert.AreEqual(5f, weapon.GetCurrentStats().damage);
        Assert.AreEqual(2, weapon.GetCurrentStats().number);
        Assert.AreEqual(3, CountSpawnedProjectiles(projectilePrefab));
        Assert.AreEqual(3f, weapon.GetCurrentCooldown());
    }
}
