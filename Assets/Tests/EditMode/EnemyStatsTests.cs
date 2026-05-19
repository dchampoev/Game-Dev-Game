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
        return (T)GetPrivateField(target, fieldName).GetValue(target);
    }

    private void SetPrivateField(object target, string fieldName, object value)
    {
        GetPrivateField(target, fieldName).SetValue(target, value);
    }

    private FieldInfo GetPrivateField(object target, string fieldName)
    {
        System.Type type = target.GetType();
        while (type != null)
        {
            FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null) return field;
            type = type.BaseType;
        }

        Assert.Fail($"Could not find field {fieldName} on {target.GetType().Name}.");
        return null;
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

    private EnemyStats CreateStartedEnemy()
    {
        EnemyStats stats = CreateEnemy();
        CallStart(stats);
        return stats;
    }

    private BuffData CreateEnemyBuff(
        BuffData.Type type = BuffData.Type.debuff,
        BuffData.ModifierType modifierType = BuffData.ModifierType.additive,
        EnemyStats.Stats enemyModifier = default)
    {
        BuffData data = ScriptableObject.CreateInstance<BuffData>();
        data.type = type;
        data.variations = new[]
        {
            new BuffData.Stats
            {
                duration = 10f,
                tickInterval = 10f,
                damagePerSecond = 0f,
                healPerSecond = 0f,
                stackType = BuffData.StackType.stacksFully,
                modifierType = modifierType,
                enemyModifier = enemyModifier
            }
        };
        return data;
    }

    private int GetActiveBuffCount(EnemyStats stats)
    {
        return GetPrivateField<System.Collections.IList>(stats, "activeBuffs").Count;
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

        SetPrivateField(stats, "sprite", renderer);
        SetPrivateField(stats, "originialColor", Color.white);
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
        TestScriptableObjectCleanup.DestroyRuntimeObjects<BuffData>();
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
        EnemyStats stats = CreateStartedEnemy();

        stats.TakeDamage(3f, Vector2.zero, 0f, 0f);

        Assert.AreEqual(7f, GetPrivateField<float>(stats, "health"));
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
        Assert.AreEqual(20f, GetPrivateField<float>(stats, "health"));
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

        Assert.AreEqual(stats.Actual.maxHealth, GetPrivateField<float>(stats, "health"));
    }

    [Test]
    public void ApplyBuff_WhenFreezeResistanceAlwaysTriggers_ShouldRejectFreezeBuff()
    {
        EnemyStats stats = CreateStartedEnemy();
        EnemyStats.Stats currentStats = stats.baseStats;
        currentStats.resistances.freeze = 1f;
        stats.baseStats = currentStats;
        stats.RecalculateStats();

        BuffData freezeBuff = CreateEnemyBuff(BuffData.Type.freeze);

        bool result = stats.ApplyBuff(freezeBuff);

        Assert.IsFalse(result);
        Assert.AreEqual(0, GetActiveBuffCount(stats));
    }

    [Test]
    public void ApplyBuff_WhenDebuffResistanceAlwaysTriggers_ShouldRejectDebuff()
    {
        EnemyStats stats = CreateStartedEnemy();
        EnemyStats.Stats currentStats = stats.baseStats;
        currentStats.resistances.debuff = 1f;
        stats.baseStats = currentStats;
        stats.RecalculateStats();

        BuffData debuff = CreateEnemyBuff(BuffData.Type.debuff);

        bool result = stats.ApplyBuff(debuff);

        Assert.IsFalse(result);
        Assert.AreEqual(0, GetActiveBuffCount(stats));
    }

    [Test]
    public void RecalculateStats_WhenAdditiveEnemyBuffIsActive_ShouldAddEnemyModifier()
    {
        EnemyStats stats = CreateStartedEnemy();
        BuffData buff = CreateEnemyBuff(
            modifierType: BuffData.ModifierType.additive,
            enemyModifier: new EnemyStats.Stats
            {
                maxHealth = 5f,
                moveSpeed = 2f,
                damage = 1f,
                knockbackMultiplier = 0.5f
            });

        bool result = stats.ApplyBuff(buff);

        Assert.IsTrue(result);
        Assert.AreEqual(15f, stats.Actual.maxHealth);
        Assert.AreEqual(4f, stats.Actual.moveSpeed);
        Assert.AreEqual(4f, stats.Actual.damage);
        Assert.AreEqual(1.5f, stats.Actual.knockbackMultiplier);
    }

    [Test]
    public void RecalculateStats_WhenMultiplicativeEnemyBuffIsActive_ShouldMultiplyEnemyModifier()
    {
        EnemyStats stats = CreateStartedEnemy();
        BuffData buff = CreateEnemyBuff(
            modifierType: BuffData.ModifierType.multiplicative,
            enemyModifier: new EnemyStats.Stats
            {
                maxHealth = 2f,
                moveSpeed = 0.5f,
                damage = 3f,
                knockbackMultiplier = 0.25f,
                resistances = new EnemyStats.Resitances { freeze = 1f, kill = 1f, debuff = 1f }
            });

        bool result = stats.ApplyBuff(buff);

        Assert.IsTrue(result);
        Assert.AreEqual(20f, stats.Actual.maxHealth);
        Assert.AreEqual(1f, stats.Actual.moveSpeed);
        Assert.AreEqual(9f, stats.Actual.damage);
        Assert.AreEqual(0.25f, stats.Actual.knockbackMultiplier);
    }
}
