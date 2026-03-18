using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using System.Reflection;

public class EnemyStatsTests
{
    private EnemyScriptableObject CreateEnemyData(
        float moveSpeed = 2f,
        float maxHealth = 10f,
        float damage = 3f)
    {
        EnemyScriptableObject data = ScriptableObject.CreateInstance<EnemyScriptableObject>();

        SerializedObject so = new SerializedObject(data);
        so.FindProperty("moveSpeed").floatValue = moveSpeed;
        so.FindProperty("maxHealth").floatValue = maxHealth;
        so.FindProperty("damage").floatValue = damage;
        so.ApplyModifiedProperties();

        return data;
    }

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

    private void CallUpdate(EnemyStats stats)
    {
        typeof(EnemyStats)
            .GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(stats, null);
    }

    private Transform GetPrivatePlayer(EnemyStats stats)
    {
        return (Transform)typeof(EnemyStats)
            .GetField("player", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(stats);
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(obj);
        }

        foreach (var enemyData in Resources.FindObjectsOfTypeAll<EnemyScriptableObject>())
        {
            Object.DestroyImmediate(enemyData);
        }
    }

    [Test]
    public void Awake_WhenEnemyDataIsNull_ShouldLeaveStatsUnchanged()
    {
        GameObject enemyObject = new GameObject("Enemy");
        EnemyStats stats = enemyObject.AddComponent<EnemyStats>();

        stats.currentMoveSpeed = 0f;
        stats.currentHealth = 0f;
        stats.currentDamage = 0f;
        stats.enemyData = null;

        CallAwake(stats);

        Assert.AreEqual(0f, stats.currentMoveSpeed);
        Assert.AreEqual(0f, stats.currentHealth);
        Assert.AreEqual(0f, stats.currentDamage);
    }

    [Test]
    public void Awake_WhenEnemyDataExists_ShouldInitializeStats()
    {
        GameObject enemyObject = new GameObject("Enemy");
        EnemyStats stats = enemyObject.AddComponent<EnemyStats>();
        stats.enemyData = CreateEnemyData(moveSpeed: 4f, maxHealth: 15f, damage: 6f);

        CallAwake(stats);

        Assert.AreEqual(4f, stats.currentMoveSpeed);
        Assert.AreEqual(15f, stats.currentHealth);
        Assert.AreEqual(6f, stats.currentDamage);
    }

    [Test]
    public void InitializeStats_ShouldCopyValuesFromEnemyData()
    {
        GameObject enemyObject = new GameObject("Enemy");
        EnemyStats stats = enemyObject.AddComponent<EnemyStats>();
        stats.enemyData = CreateEnemyData(moveSpeed: 5f, maxHealth: 20f, damage: 7f);

        stats.InitializeStats();

        Assert.AreEqual(5f, stats.currentMoveSpeed);
        Assert.AreEqual(20f, stats.currentHealth);
        Assert.AreEqual(7f, stats.currentDamage);
    }

    [Test]
    public void Start_WhenPlayerDoesNotExist_ShouldKeepPlayerNull()
    {
        GameObject enemyObject = new GameObject("Enemy");
        EnemyStats stats = enemyObject.AddComponent<EnemyStats>();

        CallStart(stats);

        Assert.IsNull(GetPrivatePlayer(stats));
    }

    [Test]
    public void Start_WhenPlayerExists_ShouldCachePlayerTransform()
    {
        GameObject playerObject = new GameObject("Player");
        playerObject.AddComponent<PlayerStats>();

        GameObject enemyObject = new GameObject("Enemy");
        EnemyStats stats = enemyObject.AddComponent<EnemyStats>();

        CallStart(stats);

        Assert.AreEqual(playerObject.transform, GetPrivatePlayer(stats));
    }

    [Test]
    public void TakeDamage_WhenDamageIsLessThanHealth_ShouldOnlyReduceHealth()
    {
        GameObject enemyObject = new GameObject("Enemy");
        EnemyStats stats = enemyObject.AddComponent<EnemyStats>();
        stats.currentHealth = 10f;

        stats.TakeDamage(3f);

        Assert.AreEqual(7f, stats.currentHealth);
    }

    [Test]
    public void Update_WhenPlayerIsNull_ShouldNotThrowAndShouldNotMove()
    {
        GameObject enemyObject = new GameObject("Enemy");
        EnemyStats stats = enemyObject.AddComponent<EnemyStats>();
        enemyObject.transform.position = new Vector3(2f, 3f, 0f);

        CallUpdate(stats);

        Assert.AreEqual(new Vector3(2f, 3f, 0f), enemyObject.transform.position);
    }
}