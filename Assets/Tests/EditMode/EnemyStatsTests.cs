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

    [TearDown]
    public void TearDown()
    {
        foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(obj);
        }

        EnemyStats.count = 0;
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
    public void Start_WhenPlayerDoesNotExist_ShouldLeaveCachedFieldsNull()
    {
        GameObject enemyObject = new GameObject("Enemy");
        enemyObject.AddComponent<SpriteRenderer>();
        EnemyStats stats = enemyObject.AddComponent<EnemyStats>();

        CallStart(stats);

        Assert.IsNull(GetPrivateField<Transform>(stats, "player"));
        Assert.IsNull(GetPrivateField<EnemyMovement>(stats, "enemyMovement"));
    }

    [Test]
    public void Start_WhenPlayerExists_ShouldCachePlayerSpriteRendererColorAndMovement()
    {
        GameObject playerObject = new GameObject("Player");
        playerObject.SetActive(false);
        PlayerStats player = playerObject.AddComponent<PlayerStats>();
        player.enabled = false;
        playerObject.SetActive(true);

        GameObject enemyObject = new GameObject("Enemy");

        SpriteRenderer renderer = enemyObject.AddComponent<SpriteRenderer>();
        renderer.color = Color.green;

        EnemyMovement movement = enemyObject.AddComponent<EnemyMovement>();
        movement.enabled = false;

        EnemyStats stats = enemyObject.AddComponent<EnemyStats>();

        CallStart(stats);

        Assert.AreEqual(player.transform, GetPrivateField<Transform>(stats, "player"));
        Assert.AreEqual(renderer, GetPrivateField<SpriteRenderer>(stats, "spriteRenderer"));
        Assert.AreEqual(Color.green, GetPrivateField<Color>(stats, "originalColor"));
        Assert.AreEqual(movement, GetPrivateField<EnemyMovement>(stats, "enemyMovement"));
    }

    [Test]
    public void TakeDamage_WhenDamageIsPositive_ShouldReduceHealth()
    {
        GameObject enemyObject = new GameObject("Enemy");
        enemyObject.AddComponent<SpriteRenderer>();

        EnemyMovement movement = enemyObject.AddComponent<EnemyMovement>();
        movement.enabled = false;

        EnemyStats stats = enemyObject.AddComponent<EnemyStats>();
        stats.currentHealth = 10f;

        typeof(EnemyStats)
            .GetField("spriteRenderer", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(stats, enemyObject.GetComponent<SpriteRenderer>());

        typeof(EnemyStats)
            .GetField("originalColor", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(stats, Color.white);

        typeof(EnemyStats)
            .GetField("enemyMovement", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(stats, movement);

        stats.TakeDamage(3f, Vector2.zero, 0f, 0f);

        Assert.AreEqual(7f, stats.currentHealth);
    }
}