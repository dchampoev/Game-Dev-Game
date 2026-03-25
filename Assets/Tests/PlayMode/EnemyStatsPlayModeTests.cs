using System.Collections;
using System.Collections.Generic;
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

    private EnemySpawner CreateDisabledSpawnerWithSingleSpawnPoint(Vector3 spawnOffset)
    {
        GameObject spawnerObject = new GameObject("Spawner");
        EnemySpawner spawner = spawnerObject.AddComponent<EnemySpawner>();
        spawner.enabled = false;
        spawner.relativeSpawnPoints = new List<Transform>();

        GameObject spawnPointObject = new GameObject("SpawnPoint");
        spawnPointObject.transform.position = spawnOffset;
        spawner.relativeSpawnPoints.Add(spawnPointObject.transform);

        return spawner;
    }

    private EnemyStats CreateEnemy(Color initialColor, float health = 10f, float flash = 0.1f, float fade = 0.1f)
    {
        GameObject enemyObject = new GameObject("Enemy");
        SpriteRenderer renderer = enemyObject.AddComponent<SpriteRenderer>();
        renderer.color = initialColor;

        EnemyStats stats = enemyObject.AddComponent<EnemyStats>();
        stats.currentHealth = health;
        stats.damageFlashDuration = flash;
        stats.deathFadeDuration = fade;

        SetPrivateField(stats, "spriteRenderer", renderer);
        SetPrivateField(stats, "originalColor", initialColor);

        return stats;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.Destroy(obj);
        }

        yield return null;
    }

    [UnityTest]
    public IEnumerator TakeDamage_ShouldFlashAndRestoreColor()
    {
        EnemyStats stats = CreateEnemy(Color.white, 10f, 0.1f);
        SpriteRenderer renderer = stats.GetComponent<SpriteRenderer>();
        stats.damageColor = Color.red;

        stats.TakeDamage(1f, Vector2.zero, 0f, 0f);

        Assert.AreEqual(Color.red, renderer.color);

        yield return new WaitForSeconds(0.15f);

        Assert.AreEqual(Color.white, renderer.color);
        Assert.AreEqual(9f, stats.currentHealth);
    }

    [UnityTest]
    public IEnumerator TakeDamage_ShouldKillEnemyAndDestroyIt()
    {
        EnemySpawner spawner = CreateDisabledSpawnerWithSingleSpawnPoint(Vector3.zero);
        spawner.enemiesAlive = 1;

        EnemyStats stats = CreateEnemy(Color.white, 1f, 0.05f, 0.1f);
        GameObject enemyObject = stats.gameObject;

        stats.TakeDamage(1f, Vector2.zero, 0f, 0f);

        Assert.AreEqual(0, spawner.enemiesAlive);

        yield return new WaitForSeconds(0.2f);
        yield return null;

        Assert.IsTrue(enemyObject == null);
    }

    [UnityTest]
    public IEnumerator Update_ShouldRelocateEnemyNearPlayer()
    {
        CreateDisabledSpawnerWithSingleSpawnPoint(new Vector3(3f, 4f, 0f));

        EnemyStats stats = CreateEnemy(Color.white);
        GameObject enemyObject = stats.gameObject;
        stats.relocateDistance = 5f;

        GameObject player = new GameObject("Player");
        player.transform.position = new Vector3(5f, 5f, 0f);

        SetPrivateField(stats, "player", player.transform);

        enemyObject.transform.position = new Vector3(-100f, -100f, 0f);

        yield return null;

        stats.SendMessage("Update");

        Assert.AreEqual(new Vector3(8f, 9f, 0f), enemyObject.transform.position);
    }
}