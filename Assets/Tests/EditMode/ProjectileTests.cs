using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.TestTools;

public class ProjectileTests
{
    private class TestWeapon : Weapon
    {
        public Stats stats;
        public float damage = 2f;

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
    }

    private class TestProjectile : Projectile
    {
        public void CallStart()
        {
            typeof(Projectile)
                .GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(this, null);
        }

        public void CallFixedUpdate()
        {
            typeof(Projectile)
                .GetMethod("FixedUpdate", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(this, null);
        }

        public void CallOnTriggerEnter2D(Collider2D collider)
        {
            typeof(Projectile)
                .GetMethod("OnTriggerEnter2D", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(this, new object[] { collider });
        }
    }

    private void SetPrivateField(object target, string fieldName, object value)
    {
        target.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(target, value);
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
        inventory.upgradeUIOptions = new List<PlayerInventory.UpgradeUI>();

        GameObject healthBarGO = new GameObject("HealthBar");
        Image healthBar = healthBarGO.AddComponent<Image>();

        GameObject expBarGO = new GameObject("ExpBar");
        Image expBar = expBarGO.AddComponent<Image>();

        GameObject levelTextGO = new GameObject("LevelText");
        TextMeshProUGUI levelText = levelTextGO.AddComponent<TextMeshProUGUI>();

        stats.healthBar = healthBar;
        stats.expBar = expBar;
        stats.levelText = levelText;

        CharacterData characterData = ScriptableObject.CreateInstance<CharacterData>();
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

        SetPrivateField(stats, "characterData", characterData);
        SetPrivateField(stats, "inventory", inventory);
        SetPrivateField(stats, "collector", collector);
        SetPrivateField(stats, "health", 20f);

        return stats;
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(obj);
        }

        foreach (var data in Resources.FindObjectsOfTypeAll<CharacterData>())
        {
            Object.DestroyImmediate(data, true);
        }
    }

    [Test]
    public void AcquireAutoAimFacing_WhenEnemyExists_ShouldFaceEnemy()
    {
        GameObject projectileObject = new GameObject("Projectile");
        TestProjectile projectile = projectileObject.AddComponent<TestProjectile>();
        projectile.transform.position = Vector3.zero;

        GameObject enemyObject = new GameObject("Enemy");
        enemyObject.transform.position = new Vector3(10f, 0f, 0f);
        enemyObject.AddComponent<EnemyStats>();

        projectile.AcquireAutoAimFacing();

        float z = projectile.transform.eulerAngles.z;

        Assert.That(Mathf.Abs(Mathf.DeltaAngle(z, 0f)), Is.LessThan(0.1f));
    }

    [Test]
    public void AcquireAutoAimFacing_WhenNoEnemies_ShouldSetValidRotation()
    {
        GameObject projectileObject = new GameObject("Projectile");
        TestProjectile projectile = projectileObject.AddComponent<TestProjectile>();

        projectile.AcquireAutoAimFacing();

        float z = projectile.transform.eulerAngles.z;

        Assert.IsFalse(float.IsNaN(z));
    }

    [Test]
    public void Start_WhenKinematicBody_ShouldSetScaleAndPiercing()
    {
        PlayerStats owner = CreatePlayer();

        GameObject weaponObject = new GameObject("Weapon");
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();
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
        projectileObject.transform.localScale = new Vector3(1f, 1f, 1f);

        Rigidbody2D rb = projectileObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        TestProjectile projectile = projectileObject.AddComponent<TestProjectile>();
        projectile.weapon = weapon;
        projectile.owner = owner;

        projectile.CallStart();

        Assert.AreEqual(2f, projectile.transform.localScale.x);
        Assert.AreEqual(2f, projectile.transform.localScale.y);

        int piercing = (int)typeof(Projectile)
            .GetField("piercing", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(projectile);

        Assert.AreEqual(3, piercing);
    }

    [Test]
    public void FixedUpdate_WhenBodyIsKinematic_ShouldMoveProjectileForward()
    {
        PlayerStats owner = CreatePlayer();

        GameObject weaponObject = new GameObject("Weapon");
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();
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

        projectile.CallStart();

        Vector3 before = projectile.transform.position;

        projectile.CallFixedUpdate();

        Vector3 after = projectile.transform.position;

        Assert.Greater(after.x, before.x);
    }
    [Test]
    public void OnTriggerEnter2D_WhenEnemyHitAndPiercingBecomesZero_ShouldDamageEnemyAndDestroyProjectile()
    {
        GameObject playerObject = new GameObject("Player");
        playerObject.SetActive(false);
        playerObject.tag = "Player";
        playerObject.AddComponent<BoxCollider2D>();

        PlayerMovement playerMovement = playerObject.AddComponent<PlayerMovement>();
        playerMovement.enabled = false;

        PlayerStats playerStats = playerObject.AddComponent<PlayerStats>();
        playerStats.enabled = false;

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

        GameObject projectileObject = new GameObject("Projectile");
        Rigidbody2D rb = projectileObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        TestProjectile projectile = projectileObject.AddComponent<TestProjectile>();
        projectile.weapon = weapon;
        projectile.CallStart();

        GameObject enemyObject = new GameObject("Enemy");
        enemyObject.tag = "Enemy";

        BoxCollider2D enemyCollider = enemyObject.AddComponent<BoxCollider2D>();

        SpriteRenderer spriteRenderer = enemyObject.AddComponent<SpriteRenderer>();
        spriteRenderer.color = Color.white;

        EnemyMovement enemyMovement = enemyObject.AddComponent<EnemyMovement>();
        enemyMovement.enabled = false;

        EnemyStats enemyStats = enemyObject.AddComponent<EnemyStats>();
        enemyStats.currentHealth = 10f;
        enemyStats.currentDamage = 1f;
        enemyStats.currentMoveSpeed = 1f;

        SetPrivateField(enemyStats, "spriteRenderer", spriteRenderer);
        SetPrivateField(enemyStats, "originalColor", Color.white);
        SetPrivateField(enemyStats, "enemyMovement", enemyMovement);

        LogAssert.Expect(
            LogType.Error,
            "Destroy may not be called from edit mode! Use DestroyImmediate instead.\nDestroying an object in edit mode destroys it permanently."
        );

        projectile.CallOnTriggerEnter2D(enemyCollider);

        Assert.AreEqual(8f, enemyStats.currentHealth);

        int piercing = (int)typeof(Projectile)
            .GetField("piercing", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(projectile);

        Assert.AreEqual(0, piercing);
    }
}