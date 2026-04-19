using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProjectileWeaponPlayModeTests
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

    private void SetPrivateField(object target, string fieldName, object value)
    {
        target.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(target, value);
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

        GameObject collectorGO = new GameObject("Collector");
        collectorGO.transform.SetParent(playerObject.transform);
        collectorGO.AddComponent<CircleCollider2D>();
        PlayerCollector collector = collectorGO.AddComponent<PlayerCollector>();
        collector.enabled = false;

        GameObject inventoryGO = new GameObject("Inventory");
        inventoryGO.transform.SetParent(playerObject.transform);
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

        owner.healthBar = healthBar;
        owner.expBar = expBar;
        owner.levelText = levelText;

        CharacterData characterData = ScriptableObject.CreateInstance<CharacterData>();
        createdScriptableObjects.Add(characterData);

        CharacterData.Stats stats = new CharacterData.Stats
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

        owner.baseStats = stats;
        owner.Stats = stats;
        owner.CurrentHealth = 20f;

        SetPrivateField(owner, "characterData", characterData);
        SetPrivateField(owner, "inventory", inventory);
        SetPrivateField(owner, "collector", collector);
        SetPrivateField(owner, "health", 20f);

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

    private TestProjectileWeapon CreateInactiveWeapon()
    {
        GameObject weaponObject = new GameObject("Weapon");
        weaponObject.SetActive(false);
        return weaponObject.AddComponent<TestProjectileWeapon>();
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
    public void Attack_WhenProjectilePrefabExists_ShouldSpawnProjectileAndSetCooldown()
    {
        PlayerMovement movement;
        PlayerStats owner = CreateInactiveOwner(out movement);

        GameObject projectilePrefabObject = new GameObject("ProjectilePrefab");
        TestProjectile projectilePrefab = projectilePrefabObject.AddComponent<TestProjectile>();

        WeaponData data = CreateWeaponData(2f, 0.5f);

        TestProjectileWeapon weapon = CreateInactiveWeapon();
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
    }

    [Test]
    public void Attack_WhenAttackCountIsGreaterThanOne_ShouldQueueNextAttack()
    {
        PlayerMovement movement;
        PlayerStats owner = CreateInactiveOwner(out movement);

        GameObject projectilePrefabObject = new GameObject("ProjectilePrefab");
        TestProjectile projectilePrefab = projectilePrefabObject.AddComponent<TestProjectile>();

        WeaponData data = CreateWeaponData(2f, 0.75f);

        TestProjectileWeapon weapon = CreateInactiveWeapon();
        weapon.data = data;
        weapon.SetOwner(owner);
        weapon.SetMovement(movement);
        weapon.SetCurrentCooldown(0f);
        weapon.SetCurrentStats(new Weapon.Stats
        {
            projectilePrefab = projectilePrefab,
            cooldown = 2f,
            projectileInterval = 0.75f,
            spawnVariance = new Rect(0f, 0f, 0f, 0f)
        });

        bool result = weapon.CallAttack(3);

        Assert.IsTrue(result);
        Assert.AreEqual(2, weapon.GetCurrentAttackCount());
        Assert.AreEqual(0.75f, weapon.GetCurrentAttackInterval());
    }

    [Test]
    public void Update_WhenQueuedAttackIntervalExpires_ShouldFireNextProjectile()
    {
        PlayerMovement movement;
        PlayerStats owner = CreateInactiveOwner(out movement);

        GameObject projectilePrefabObject = new GameObject("ProjectilePrefab");
        TestProjectile projectilePrefab = projectilePrefabObject.AddComponent<TestProjectile>();

        WeaponData data = CreateWeaponData(2f, 0.01f);

        TestProjectileWeapon weapon = CreateInactiveWeapon();
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
            projectileInterval = 0.01f,
            spawnVariance = new Rect(0f, 0f, 0f, 0f)
        });

        weapon.CallUpdate();

        TestProjectile[] projectiles = Object.FindObjectsByType<TestProjectile>(FindObjectsSortMode.None);

        Assert.AreEqual(2, projectiles.Length);
    }
}