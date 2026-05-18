using System.Reflection;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EnemyMovementPlayModeTests
{
    private class TestEnemyMovement : EnemyMovement
    {
        public void CallStart()
        {
            Start();
        }

        public void CallUpdate()
        {
            Update();
        }
    }

    private void CallEnemyStatsStart(EnemyStats stats)
    {
        typeof(EnemyStats)
            .GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(stats, null);
    }

    private void SetField(object target, string fieldName, object value)
    {
        GetField(target, fieldName).SetValue(target, value);
    }

    private T GetFieldValue<T>(object target, string fieldName)
    {
        return (T)GetField(target, fieldName).GetValue(target);
    }

    private FieldInfo GetField(object target, string fieldName)
    {
        System.Type type = target.GetType();
        while (type != null)
        {
            FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null)
            {
                return field;
            }

            type = type.BaseType;
        }

        throw new System.MissingFieldException(target.GetType().Name, fieldName);
    }

    private EnemyStats CreateStats(float knockbackMultiplier)
    {
        GameObject statsObject = new GameObject("EnemyStats");
        statsObject.AddComponent<SpriteRenderer>();

        EnemyStats stats = statsObject.AddComponent<EnemyStats>();
        EnemyStats.Stats actualStats = new EnemyStats.Stats
        {
            maxHealth = 10f,
            moveSpeed = 1f,
            damage = 3f,
            knockbackMultiplier = knockbackMultiplier,
            resistances = new EnemyStats.Resitances()
        };

        SetField(stats, "actualStats", actualStats);
        return stats;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.Destroy(go);
        }

        yield return null;

        SpawnManager.instance = null;
    }

    [UnityTest]
    public IEnumerator Update_ShouldMoveEnemyTowardsPlayer()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        cameraObject.tag = "MainCamera";

        GameObject spawnManagerObject = new GameObject("SpawnManager");
        SpawnManager spawnManager = spawnManagerObject.AddComponent<SpawnManager>();
        spawnManager.enabled = false;
        spawnManager.referenceCamera = camera;
        SpawnManager.instance = spawnManager;

        GameObject playerObject = new GameObject("Player");
        PlayerMovement playerMovement = playerObject.AddComponent<PlayerMovement>();
        playerMovement.enabled = false;
        playerObject.transform.position = new Vector2(10f, 0f);

        GameObject enemyObject = new GameObject("Enemy");
        TestEnemyMovement movement = enemyObject.AddComponent<TestEnemyMovement>();

        EnemyStats enemyStats = enemyObject.AddComponent<EnemyStats>();
        enemyStats.baseStats = new EnemyStats.Stats
        {
            maxHealth = 10f,
            moveSpeed = 5f,
            damage = 3f,
            knockbackMultiplier = 1f,
            resistances = new EnemyStats.Resitances()
        };

        CallEnemyStatsStart(enemyStats);

        enemyObject.transform.position = Vector2.zero;

        movement.CallStart();

        Vector2 before = enemyObject.transform.position;

        yield return null;

        movement.CallUpdate();

        Vector2 after = enemyObject.transform.position;

        Assert.AreNotEqual(before, after);

        yield return null;
    }

    [UnityTest]
    public IEnumerator Move_WhenRigidbodyExists_ShouldMoveRigidbodyTowardsPlayer()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        cameraObject.tag = "MainCamera";

        GameObject spawnManagerObject = new GameObject("SpawnManager");
        SpawnManager spawnManager = spawnManagerObject.AddComponent<SpawnManager>();
        spawnManager.enabled = false;
        spawnManager.referenceCamera = camera;
        SpawnManager.instance = spawnManager;

        GameObject playerObject = new GameObject("Player");
        PlayerMovement playerMovement = playerObject.AddComponent<PlayerMovement>();
        playerMovement.enabled = false;
        playerObject.transform.position = new Vector2(10f, 0f);

        GameObject enemyObject = new GameObject("Enemy");
        Rigidbody2D rigidBody = enemyObject.AddComponent<Rigidbody2D>();
        rigidBody.bodyType = RigidbodyType2D.Kinematic;

        EnemyStats enemyStats = enemyObject.AddComponent<EnemyStats>();
        enemyStats.baseStats = new EnemyStats.Stats
        {
            maxHealth = 10f,
            moveSpeed = 5f,
            damage = 3f,
            knockbackMultiplier = 1f,
            resistances = new EnemyStats.Resitances()
        };

        CallEnemyStatsStart(enemyStats);

        TestEnemyMovement movement = enemyObject.AddComponent<TestEnemyMovement>();
        movement.CallStart();

        Vector2 before = rigidBody.position;

        movement.Move();
        yield return new WaitForFixedUpdate();

        Vector2 after = rigidBody.position;

        Assert.Greater(after.x, before.x);
        Assert.AreEqual(before.y, after.y, 0.001f);
    }

    [Test]
    public void Knockback_WhenVelocityVarianceEnabled_ShouldReduceVelocityOnly()
    {
        GameObject enemyObject = new GameObject("Enemy");
        TestEnemyMovement movement = enemyObject.AddComponent<TestEnemyMovement>();
        SetField(movement, "stats", CreateStats(knockbackMultiplier: 0.25f));
        movement.knockbackVariance = EnemyMovement.KnockbackVariance.velocity;

        movement.Knockback(new Vector2(8f, 0f), 4f);

        Assert.AreEqual(new Vector2(2f, 0f), GetFieldValue<Vector2>(movement, "knockbackVelocity"));
        Assert.AreEqual(4f, GetFieldValue<float>(movement, "knockbackDuration"));
    }

    [Test]
    public void Knockback_WhenDurationAndVelocityVarianceEnabled_ShouldSplitReduction()
    {
        GameObject enemyObject = new GameObject("Enemy");
        TestEnemyMovement movement = enemyObject.AddComponent<TestEnemyMovement>();
        SetField(movement, "stats", CreateStats(knockbackMultiplier: 0.25f));
        movement.knockbackVariance = EnemyMovement.KnockbackVariance.velocity | EnemyMovement.KnockbackVariance.duration;

        movement.Knockback(new Vector2(8f, 0f), 4f);

        Assert.AreEqual(new Vector2(4f, 0f), GetFieldValue<Vector2>(movement, "knockbackVelocity"));
        Assert.AreEqual(2f, GetFieldValue<float>(movement, "knockbackDuration"));
    }

    [Test]
    public void Knockback_WhenVarianceDisabled_ShouldNotApplyKnockback()
    {
        GameObject enemyObject = new GameObject("Enemy");
        TestEnemyMovement movement = enemyObject.AddComponent<TestEnemyMovement>();
        SetField(movement, "stats", CreateStats(knockbackMultiplier: 0.25f));
        movement.knockbackVariance = 0;

        movement.Knockback(new Vector2(8f, 0f), 4f);

        Assert.AreEqual(Vector2.zero, GetFieldValue<Vector2>(movement, "knockbackVelocity"));
        Assert.AreEqual(0f, GetFieldValue<float>(movement, "knockbackDuration"));
    }
}
