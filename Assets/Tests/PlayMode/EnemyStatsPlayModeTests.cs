using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EnemyStatsPlayModeTests
{
    private float GetCurrentHealth(EnemyStats stats)
    {
        return (float)typeof(EnemyStats)
            .GetField("currentHealth", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(stats);
    }

    private EnemyStats CreateEnemy(Color initialColor, float health = 10f, float flash = 0.1f, float fade = 0.1f)
    {
        GameObject enemyObject = new GameObject("Enemy");

        SpriteRenderer renderer = enemyObject.AddComponent<SpriteRenderer>();
        renderer.color = initialColor;

        EnemyMovement movement = enemyObject.AddComponent<EnemyMovement>();
        movement.enabled = false;

        EnemyStats stats = enemyObject.AddComponent<EnemyStats>();
        stats.baseStats = new EnemyStats.Stats
        {
            maxHealth = health,
            moveSpeed = 1f,
            damage = 3f,
            knockbackMultiplier = 1f,
            resistances = new EnemyStats.Resitances()
        };

        stats.damageFlashDuration = flash;
        stats.deathFadeDuration = fade;

        return stats;
    }

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        Time.timeScale = 1f;
        EnemyStats.count = 0;
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Time.timeScale = 1f;

        foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.Destroy(obj);
        }

        yield return null;

        SpawnManager.instance = null;
        EnemyStats.count = 0;
    }

    [UnityTest]
    public IEnumerator TakeDamage_ShouldFlashAndRestoreColor()
    {
        EnemyStats stats = CreateEnemy(Color.white, 10f, 0.05f);
        SpriteRenderer renderer = stats.GetComponent<SpriteRenderer>();
        stats.damageColor = Color.red;

        yield return null;

        stats.TakeDamage(1f, Vector2.zero, 0f, 0f);

        Assert.AreEqual(Color.red, renderer.color);

        yield return new WaitForSeconds(0.15f);

        Assert.AreEqual(Color.white, renderer.color);
        Assert.AreEqual(9f, GetCurrentHealth(stats));
    }

    [UnityTest]
    public IEnumerator Awake_ShouldIncreaseEnemyCount()
    {
        Assert.AreEqual(0, EnemyStats.count);

        CreateEnemy(Color.white);

        yield return null;

        Assert.AreEqual(1, EnemyStats.count);
    }

    [UnityTest]
    public IEnumerator OnDestroy_ShouldDecreaseEnemyCount()
    {
        EnemyStats stats = CreateEnemy(Color.white);

        yield return null;

        Assert.AreEqual(1, EnemyStats.count);

        Object.Destroy(stats.gameObject);

        yield return null;

        Assert.AreEqual(0, EnemyStats.count);
    }

    [UnityTest]
    public IEnumerator TakeDamage_WhenKnockbackForceIsZero_ShouldNotRequireKnockback()
    {
        EnemyStats stats = CreateEnemy(Color.white, 10f);

        yield return null;

        stats.TakeDamage(2f, Vector2.zero, 0f, 0f);

        yield return null;

        Assert.AreEqual(8f, GetCurrentHealth(stats));
    }
}