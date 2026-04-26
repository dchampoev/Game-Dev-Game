using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TMPro;

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

        foreach (var data in Resources.FindObjectsOfTypeAll<WeaponData>())
        {
            Object.DestroyImmediate(data, true);
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

        PlayerStats owner = CreatePlayer();
        weapon.SetOwner(owner);

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
            projectilePrefab = null,
            cooldown = 2f
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