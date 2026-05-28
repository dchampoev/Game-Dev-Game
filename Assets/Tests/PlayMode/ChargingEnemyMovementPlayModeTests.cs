using System.Reflection;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ChargingEnemyMovementPlayModeTests
{
    private class TestChargingEnemyMovement : ChargingEnemyMovement
    {
        public void CallStart()
        {
            Start();
        }

        public void CallMove()
        {
            Move();
        }
    }

    private void CallEnemyStatsStart(EnemyStats stats)
    {
        typeof(EnemyStats)
            .GetMethod("Start", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            .Invoke(stats, null);
    }

    private void CreateSpawnManager()
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
    }

    private GameObject CreatePlayer(Vector3 position)
    {
        GameObject playerObject = new GameObject("Player");
        playerObject.transform.position = position;

        PlayerMovement playerMovement = playerObject.AddComponent<PlayerMovement>();
        playerMovement.enabled = false;

        return playerObject;
    }

    private TestChargingEnemyMovement CreateEnemy(float speed)
    {
        GameObject enemyObject = new GameObject("Enemy");
        enemyObject.transform.position = Vector3.zero;

        Rigidbody2D rb = enemyObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Dynamic;

        EnemyStats enemyStats = enemyObject.AddComponent<EnemyStats>();
        enemyStats.baseStats = new EnemyStats.Stats
        {
            maxHealth = 10f,
            moveSpeed = speed,
            damage = 3f,
            knockbackMultiplier = 1f,
            resistances = new EnemyStats.Resistances()
        };

        TestChargingEnemyMovement movement = enemyObject.AddComponent<TestChargingEnemyMovement>();

        return movement;
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
        EnemyStats.count = 0;
    }

    [UnityTest]
    public IEnumerator Start_ShouldSetChargeDirectionTowardsPlayer()
    {
        CreateSpawnManager();
        CreatePlayer(new Vector3(10f, 0f, 0f));

        TestChargingEnemyMovement movement = CreateEnemy(5f);
        Rigidbody2D rb = movement.GetComponent<Rigidbody2D>();

        yield return null;

        movement.CallStart();
        movement.CallMove();

        Assert.Greater(rb.linearVelocity.x, 0f);
        Assert.That(Mathf.Abs(rb.linearVelocity.y), Is.LessThan(0.01f));
    }

    [UnityTest]
    public IEnumerator Move_ShouldMoveEnemyUsingCurrentMoveSpeed()
    {
        CreateSpawnManager();
        CreatePlayer(new Vector3(10f, 0f, 0f));

        TestChargingEnemyMovement movement = CreateEnemy(10f);
        Rigidbody2D rb = movement.GetComponent<Rigidbody2D>();

        yield return null;

        movement.CallStart();
        movement.CallMove();

        Assert.Greater(rb.linearVelocity.x, 0f);
        Assert.AreEqual(10f, rb.linearVelocity.magnitude, 0.2f);
    }
}
