using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TMPro;

public class ProjectilePlayModeTests
{
    private class TestWeapon : Weapon
    {
        public Stats stats;
        public float damage = 2f;

        protected override void Update()
        {
        }

        public override Stats GetStats()
        {
            return stats;
        }

        public override float GetDamage()
        {
            return damage;
        }

        public override float GetArea()
        {
            return stats.area;
        }

        public override float GetSpeed()
        {
            return stats.speed;
        }

        public override float GetLifespan()
        {
            return stats.lifespan;
        }
    }

    private class TestProjectile : Projectile
    {
        protected override void Start()
        {
        }

        protected override void FixedUpdate()
        {
        }

        public void CallBaseStart()
        {
            base.Start();
        }

        public void CallBaseFixedUpdate()
        {
            base.FixedUpdate();
        }

        public void CallOnTriggerEnter2D(Collider2D collider)
        {
            typeof(Projectile)
                .GetMethod("OnTriggerEnter2D", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(this, new object[] { collider });
        }
    }

    private float GetCurrentHealth(EnemyStats enemy)
    {
        return (float)typeof(EntityStats)
            .GetField("health", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(enemy);
    }

    private PlayerStats CreatePlayer()
    {
        GameObject playerGO = new GameObject("Player");
        playerGO.SetActive(false);

        PlayerStats stats = playerGO.AddComponent<PlayerStats>();
        stats.enabled = false;

        PlayerMovement movement = playerGO.AddComponent<PlayerMovement>();
        movement.enabled = false;
        movement.lastMoveDirection = Vector2.right;

        GameObject collectorGO = new GameObject("Collector");
        collectorGO.transform.SetParent(playerGO.transform);
        collectorGO.AddComponent<CircleCollider2D>();
        PlayerCollector collector = collectorGO.AddComponent<PlayerCollector>();
        collector.enabled = false;

        GameObject inventoryGO = new GameObject("Inventory");
        inventoryGO.transform.SetParent(playerGO.transform);
        PlayerInventory inventory = inventoryGO.AddComponent<PlayerInventory>();
        inventory.weaponSlots = new List<PlayerInventory.Slot>();
        inventory.passiveSlots = new List<PlayerInventory.Slot>();
        inventory.availableWeapons = new List<WeaponData>();
        inventory.availablePassives = new List<PassiveData>();

        stats.healthBar = new GameObject("HealthBar").AddComponent<Image>();
        stats.expBar = new GameObject("ExpBar").AddComponent<Image>();
        stats.levelText = new GameObject("LevelText").AddComponent<TextMeshProUGUI>();

        CharacterData.Stats playerStats = new CharacterData.Stats
        {
            maxHealth = 20f,
            recovery = 1f,
            armor = 0f,
            moveSpeed = 5f,
            might = 1f,
            area = 1f,
            speed = 1f,
            duration = 1f,
            amount = 0,
            cooldown = 1f,
            luck = 1f,
            growth = 1f,
            greed = 1f,
            curse = 0f,
            magnet = 1f,
            revival = 0
        };

        stats.baseStats = playerStats;
        stats.Stats = playerStats;
        stats.CurrentHealth = 20f;

        typeof(PlayerStats).GetField("inventory", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(stats, inventory);
        typeof(PlayerStats).GetField("collector", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(stats, collector);
        typeof(PlayerStats).GetField("health", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(stats, 20f);

        return stats;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.Destroy(obj);
        }

        yield return null;

        EnemyStats.count = 0;
        SpawnManager.instance = null;
    }

    [UnityTest]
    public IEnumerator AcquireAutoAimFacing_WhenEnemyExists_ShouldFaceEnemy()
    {
        GameObject projectileObject = new GameObject("Projectile");
        TestProjectile projectile = projectileObject.AddComponent<TestProjectile>();
        projectile.transform.position = Vector3.zero;

        GameObject enemyObject = new GameObject("Enemy");
        enemyObject.transform.position = new Vector3(10f, 0f, 0f);
        enemyObject.AddComponent<SpriteRenderer>();
        enemyObject.AddComponent<EnemyStats>();

        projectile.AcquireAutoAimFacing();

        float z = projectile.transform.eulerAngles.z;

        Assert.That(Mathf.Abs(Mathf.DeltaAngle(z, 0f)), Is.LessThan(0.1f));

        yield return null;
    }

    [UnityTest]
    public IEnumerator AcquireAutoAimFacing_WhenNoEnemies_ShouldSetValidRotation()
    {
        GameObject projectileObject = new GameObject("Projectile");
        TestProjectile projectile = projectileObject.AddComponent<TestProjectile>();

        projectile.AcquireAutoAimFacing();

        float z = projectile.transform.eulerAngles.z;

        Assert.IsFalse(float.IsNaN(z));

        yield return null;
    }

    [UnityTest]
    public IEnumerator Start_WhenKinematicBody_ShouldSetScaleAndPiercing()
    {
        PlayerStats owner = CreatePlayer();

        GameObject weaponObject = new GameObject("Weapon");
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();
        weapon.enabled = false;
        weapon.stats = new Weapon.Stats
        {
            speed = 5f,
            area = 2f,
            piercing = 3,
            lifespan = 0f
        };

        typeof(Item)
            .GetField("owner", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(weapon, owner);

        GameObject projectileObject = new GameObject("Projectile");
        projectileObject.transform.localScale = Vector3.one;

        Rigidbody2D rb = projectileObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        TestProjectile projectile = projectileObject.AddComponent<TestProjectile>();
        projectile.weapon = weapon;
        projectile.owner = owner;

        projectile.CallBaseStart();

        Assert.AreEqual(2f, projectile.transform.localScale.x);
        Assert.AreEqual(2f, projectile.transform.localScale.y);

        int piercing = (int)typeof(Projectile)
            .GetField("piercing", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(projectile);

        Assert.AreEqual(3, piercing);

        yield return null;
    }

    [UnityTest]
    public IEnumerator FixedUpdate_WhenBodyIsKinematic_ShouldMoveProjectileForward()
    {
        PlayerStats owner = CreatePlayer();

        GameObject weaponObject = new GameObject("Weapon");
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();
        weapon.enabled = false;
        weapon.stats = new Weapon.Stats
        {
            speed = 10f,
            area = 1f,
            piercing = 1,
            lifespan = 0f
        };

        typeof(Item)
            .GetField("owner", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(weapon, owner);

        GameObject projectileObject = new GameObject("Projectile");
        Rigidbody2D rb = projectileObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        TestProjectile projectile = projectileObject.AddComponent<TestProjectile>();
        projectile.weapon = weapon;
        projectile.owner = owner;

        projectile.CallBaseStart();

        Vector3 before = projectile.transform.position;

        yield return new WaitForFixedUpdate();

        projectile.CallBaseFixedUpdate();

        Vector3 after = projectile.transform.position;

        Assert.Greater(after.x, before.x);
    }

    [UnityTest]
    public IEnumerator OnTriggerEnter2D_WhenEnemyHitAndPiercingBecomesZero_ShouldDamageEnemyAndDestroyProjectile()
    {
        PlayerStats owner = CreatePlayer();

        GameObject weaponObject = new GameObject("Weapon");
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();
        weapon.enabled = false;
        weapon.damage = 2f;
        weapon.stats = new Weapon.Stats
        {
            speed = 0f,
            area = 1f,
            piercing = 1,
            lifespan = 0f
        };

        typeof(Item)
            .GetField("owner", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(weapon, owner);

        GameObject projectileObject = new GameObject("Projectile");
        Rigidbody2D projectileRb = projectileObject.AddComponent<Rigidbody2D>();
        projectileRb.bodyType = RigidbodyType2D.Kinematic;

        TestProjectile projectile = projectileObject.AddComponent<TestProjectile>();
        projectile.weapon = weapon;
        projectile.owner = owner;
        projectile.CallBaseStart();

        GameObject enemyObject = new GameObject("Enemy");
        enemyObject.tag = "Enemy";
        enemyObject.transform.position = Vector3.zero;

        BoxCollider2D enemyCollider = enemyObject.AddComponent<BoxCollider2D>();

        SpriteRenderer spriteRenderer = enemyObject.AddComponent<SpriteRenderer>();
        spriteRenderer.color = Color.white;

        Rigidbody2D enemyRb = enemyObject.AddComponent<Rigidbody2D>();
        enemyRb.gravityScale = 0f;

        EnemyMovement enemyMovement = enemyObject.AddComponent<EnemyMovement>();
        enemyMovement.enabled = false;
        enemyMovement.knockbackVariance = 0f;

        EnemyStats enemyStats = enemyObject.AddComponent<EnemyStats>();
        enemyStats.baseStats = new EnemyStats.Stats
        {
            maxHealth = 10f,
            moveSpeed = 1f,
            damage = 1f,
            knockbackMultiplier = 1f,
            resistances = new EnemyStats.Resitances()
        };

        yield return null;

        projectile.CallOnTriggerEnter2D(enemyCollider);

        yield return null;

        Assert.AreEqual(8f, GetCurrentHealth(enemyStats));
        Assert.IsTrue(projectileObject == null);
    }
}
