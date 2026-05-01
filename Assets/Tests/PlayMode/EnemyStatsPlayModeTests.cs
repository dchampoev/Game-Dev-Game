using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EnemyStatsPlayModeTests
{
    private void SetPrivateField(object obj, string fieldName, object value)
    {
        obj.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(obj, value);
    }

    private EnemyStats CreateEnemy(Color initialColor, float health = 10f, float flash = 0.1f, float fade = 0.1f)
    {
        GameObject enemyObject = new GameObject("Enemy");

        SpriteRenderer renderer = enemyObject.AddComponent<SpriteRenderer>();
        renderer.color = initialColor;

        EnemyMovement movement = enemyObject.AddComponent<EnemyMovement>();
        movement.enabled = false;

        EnemyStats stats = enemyObject.AddComponent<EnemyStats>();
        stats.currentHealth = health;
        stats.damageFlashDuration = flash;
        stats.deathFadeDuration = fade;

        SetPrivateField(stats, "spriteRenderer", renderer);
        SetPrivateField(stats, "originalColor", initialColor);
        SetPrivateField(stats, "enemyMovement", movement);

        return stats;
    }

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        EnemyStats.count = 0;
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
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

        stats.TakeDamage(1f, Vector2.zero, 0f, 0f);

        Assert.AreEqual(Color.red, renderer.color);

        yield return new WaitForSeconds(0.1f);

        Assert.AreEqual(Color.white, renderer.color);
        Assert.AreEqual(9f, stats.currentHealth);
    }

    [UnityTest]
    public IEnumerator TakeDamage_WhenHealthReachesZero_ShouldFadeAndDestroyEnemy()
    {
        EnemyStats stats = CreateEnemy(Color.white, 1f, 0.01f, 0.05f);
        GameObject enemyObject = stats.gameObject;

        stats.TakeDamage(1f, Vector2.zero, 0f, 0f);

        float timeout = 1f;
        float elapsed = 0f;

        while (enemyObject != null && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        Assert.IsTrue(enemyObject == null);
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

        stats.TakeDamage(2f, Vector2.zero, 0f, 0f);

        yield return null;

        Assert.AreEqual(8f, stats.currentHealth);
    }
}