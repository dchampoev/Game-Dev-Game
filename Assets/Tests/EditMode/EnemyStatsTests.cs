using NUnit.Framework;
using UnityEngine;

public class EnemyStatsTests
{
    [Test]
    public void InitializeStats_ShouldCopyValuesFromScriptableObject()
    {
        GameObject enemyObject = new GameObject("Enemy");
        EnemyStats stats = enemyObject.AddComponent<EnemyStats>();

        EnemyScriptableObject data = ScriptableObject.CreateInstance<EnemyScriptableObject>();

        var so = new UnityEditor.SerializedObject(data);
        so.FindProperty("moveSpeed").floatValue = 3f;
        so.FindProperty("maxHealth").floatValue = 10f;
        so.FindProperty("damage").floatValue = 2f;
        so.ApplyModifiedProperties();

        stats.enemyData = data;

        stats.InitializeStats();

        Assert.AreEqual(3f, stats.currentMoveSpeed);
        Assert.AreEqual(10f, stats.currentHealth);
        Assert.AreEqual(2f, stats.currentDamage);

        Object.DestroyImmediate(enemyObject);
        Object.DestroyImmediate(data);
    }
    [Test]
    public void TakeDamage_WhenHealthStillPositive_ShouldReduceHealth()
    {
        GameObject enemyObject = new GameObject();
        EnemyStats stats = enemyObject.AddComponent<EnemyStats>();

        stats.currentHealth = 10;

        stats.TakeDamage(3);

        Assert.AreEqual(7, stats.currentHealth);

        Object.DestroyImmediate(enemyObject);
    }
    [Test]
    public void RelocateNearPlayer_ShouldChangePosition()
    {
        GameObject enemyObject = new GameObject();
        EnemyStats stats = enemyObject.AddComponent<EnemyStats>();

        GameObject player = new GameObject();
        player.AddComponent<PlayerStats>();

        stats.relocateDistance = 1;

        stats.SendMessage("Update");

        Assert.Pass();

        Object.DestroyImmediate(enemyObject);
        Object.DestroyImmediate(player);
    }
}