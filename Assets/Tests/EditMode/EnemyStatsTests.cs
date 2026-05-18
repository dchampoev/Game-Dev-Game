using NUnit.Framework;
using UnityEngine;
using System.Reflection;

public class EnemyStatsTests
{
    private void CallAwake(EnemyStats stats)
    {
        typeof(EnemyStats)
            .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(stats, null);
    }

    private void CallStart(EnemyStats stats)
    {
        typeof(EnemyStats)
            .GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(stats, null);
    }

    private T GetPrivateField<T>(object target, string fieldName)
    {
        return (T)target.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(target);
    }

    private void SetPrivateField(object target, string fieldName, object value)
    {
        target.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(target, value);
    }

    private PlayerStats CreatePlayer(float curse, int level)
    {
        GameObject playerObject = new GameObject("Player");
        PlayerStats player = playerObject.AddComponent<PlayerStats>();
        player.enabled = false;

        CharacterData.Stats stats = new CharacterData.Stats
        {
            maxHealth = 20f,
            recovery = 0f,
            armor = 0f,
            moveSpeed = 1f,
            might = 1f,
            area = 1f,
            speed = 1f,
            duration = 1f,
            amount = 0,
            cooldown = 1f,
            luck = 1f,
            growth = 1f,
            greed = 1f,
            curse = curse,
            magnet = 1f,
            revival = 0
        };

        player.Stats = stats;
        player.level = level;
        return player;
    }

    private void CreateGameManagerWithPlayers(params PlayerStats[] players)
    {
        GameObject gameManagerObject = new GameObject("GameManager");
        gameManagerObject.SetActive(false);
        GameManager manager = gameManagerObject.AddComponent<GameManager>();
        GameManager.instance = manager;
        SetPrivateField(manager, "players", players);
    }

    private EnemyStats CreateEnemy()
    {
        GameObject enemyObject = new GameObject("Enemy");
        SpriteRenderer renderer = enemyObject.AddComponent<SpriteRenderer>();

        EnemyMovement movement = enemyObject.AddComponent<EnemyMovement>();
        movement.enabled = false;

        EnemyStats stats = enemyObject.AddComponent<EnemyStats>();
        stats.baseStats = new EnemyStats.Stats
        {
            maxHealth = 10f,
            moveSpeed = 2f,
            damage = 3f,
            knockbackMultiplier = 1f,
            resistances = new EnemyStats.Resitances()
        };

        SetPrivateField(stats, "spriteRenderer", renderer);
        SetPrivateField(stats, "originalColor", Color.white);
        SetPrivateField(stats, "enemyMovement", movement);

        return stats;
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(obj);
        }

        EnemyStats.count = 0;
        GameManager.instance = null;
    }

    [Test]
    public void Awake_ShouldIncreaseEnemyCount()
    {
        EnemyStats.count = 0;

        GameObject enemyObject = new GameObject("Enemy");
        enemyObject.AddComponent<SpriteRenderer>();
        EnemyStats stats = enemyObject.AddComponent<EnemyStats>();

        CallAwake(stats);

        Assert.AreEqual(1, EnemyStats.count);
    }

    [Test]
    public void TakeDamage_WhenDamageIsPositive_ShouldReduceHealth()
    {
        GameObject enemyObject = new GameObject("Enemy");
        SpriteRenderer renderer = enemyObject.AddComponent<SpriteRenderer>();

        EnemyMovement movement = enemyObject.AddComponent<EnemyMovement>();
        movement.enabled = false;

        EnemyStats stats = enemyObject.AddComponent<EnemyStats>();

        typeof(EnemyStats)
            .GetField("currentHealth", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(stats, 10f);

        typeof(EnemyStats)
            .GetField("spriteRenderer", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(stats, renderer);

        typeof(EnemyStats)
            .GetField("originalColor", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(stats, Color.white);

        typeof(EnemyStats)
            .GetField("enemyMovement", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(stats, movement);

        stats.TakeDamage(3f, Vector2.zero, 0f, 0f);

        float currentHealth = (float)typeof(EnemyStats)
            .GetField("currentHealth", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(stats);

        Assert.AreEqual(7f, currentHealth);
    }

    [Test]
    public void Start_ShouldApplyCurseAndLevelBoostsToActualStats()
    {
        CreateGameManagerWithPlayers(CreatePlayer(curse: 1f, level: 3));
        EnemyStats stats = CreateEnemy();
        stats.baseStats = new EnemyStats.Stats
        {
            maxHealth = 10f,
            moveSpeed = 2f,
            damage = 3f,
            knockbackMultiplier = 1f,
            resistances = new EnemyStats.Resitances(),
            curseBoosts = EnemyStats.Stats.Boostable.health | EnemyStats.Stats.Boostable.damage,
            levelBoosts = EnemyStats.Stats.Boostable.moveSpeed | EnemyStats.Stats.Boostable.knockback
        };

        CallStart(stats);

        Assert.AreEqual(20f, stats.Actual.maxHealth);
        Assert.AreEqual(6f, stats.Actual.damage);
        Assert.AreEqual(6f, stats.Actual.moveSpeed);
        Assert.AreEqual(1f / 3f, stats.Actual.knockbackMultiplier, 0.001f);
        Assert.AreEqual(20f, GetPrivateField<float>(stats, "currentHealth"));
    }

    [Test]
    public void TakeDamage_WhenKillResistanceTriggers_ShouldNotReduceHealth()
    {
        EnemyStats stats = CreateEnemy();
        EnemyStats.Stats enemyStats = stats.baseStats;
        enemyStats.resistances.kill = 1f;
        stats.baseStats = enemyStats;
        CallStart(stats);

        stats.TakeDamage(stats.Actual.maxHealth, Vector2.zero, 0f, 0f);

        Assert.AreEqual(stats.Actual.maxHealth, GetPrivateField<float>(stats, "currentHealth"));
    }
}
