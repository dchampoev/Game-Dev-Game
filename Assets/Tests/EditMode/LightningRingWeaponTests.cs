using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TMPro;

public class LightningRingWeaponTests
{
    private class TestLightningRing : LightningRingWeapon
    {
        public bool CallAttack(int attackCount = 1)
        {
            return (bool)typeof(LightningRingWeapon)
                .GetMethod("Attack", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(this, new object[] { attackCount });
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

        public void SetOwner(PlayerStats value)
        {
            typeof(Item)
                .GetField("owner", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(this, value);
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
        LogAssert.ignoreFailingMessages = false;

        foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(obj);
        }

        foreach (var data in Resources.FindObjectsOfTypeAll<WeaponData>())
        {
            Object.DestroyImmediate(data, true);
        }

        foreach (var data in Resources.FindObjectsOfTypeAll<CharacterData>())
        {
            Object.DestroyImmediate(data, true);
        }
    }

    [Test]
    public void Attack_WhenHitEffectIsNull_ShouldReturnFalse()
    {
        GameObject obj = new GameObject();
        TestLightningRing weapon = obj.AddComponent<TestLightningRing>();

        PlayerStats owner = CreatePlayer();
        weapon.SetOwner(owner);

        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
        data.baseStats = new Weapon.Stats { cooldown = 2f };
        weapon.data = data;

        weapon.SetCurrentStats(new Weapon.Stats
        {
            hitEffect = null,
            cooldown = 2f
        });

        LogAssert.Expect(LogType.Warning, "Lightning Ring weapon has no projectile prefab.");

        bool result = weapon.CallAttack(1);

        Assert.IsFalse(result);
        Assert.AreEqual(2f, weapon.GetCurrentCooldown());
    }

    [Test]
    public void Attack_WhenMultipleShots_ShouldQueueNextAttack()
    {
        GameObject obj = new GameObject();
        TestLightningRing weapon = obj.AddComponent<TestLightningRing>();

        PlayerStats owner = CreatePlayer();
        weapon.SetOwner(owner);

        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
        data.baseStats = new Weapon.Stats
        {
            cooldown = 2f,
            projectileInterval = 0.5f
        };
        weapon.data = data;

        GameObject fxObject = new GameObject("Effect");
        ParticleSystem fx = fxObject.AddComponent<ParticleSystem>();

        weapon.SetCurrentStats(new Weapon.Stats
        {
            hitEffect = fx,
            projectileInterval = 0.5f,
            cooldown = 2f
        });

        GameObject enemyObject = new GameObject("Enemy");
        enemyObject.tag = "Enemy";
        enemyObject.AddComponent<SpriteRenderer>();
        EnemyStats enemy = enemyObject.AddComponent<EnemyStats>();
        enemy.currentHealth = 10f;
        enemy.currentDamage = 1f;
        enemy.currentMoveSpeed = 1f;

        weapon.SetCurrentCooldown(0f);

        LogAssert.ignoreFailingMessages = true;
        bool result = weapon.CallAttack(3);

        Assert.IsTrue(result);
        Assert.AreEqual(2, weapon.GetCurrentAttackCount());
        Assert.AreEqual(0.5f, weapon.GetCurrentAttackInterval());
    }
}