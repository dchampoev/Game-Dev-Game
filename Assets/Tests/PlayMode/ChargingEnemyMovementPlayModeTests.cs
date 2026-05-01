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

        EnemyStats enemyStats = enemyObject.AddComponent<EnemyStats>();
        enemyStats.currentMoveSpeed = speed;

        TestChargingEnemyMovement movement = enemyObject.AddComponent<TestChargingEnemyMovement>();
        movement.enabled = false;

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

        movement.CallStart();

        Vector3 before = movement.transform.position;

        movement.CallMove();

        Vector3 after = movement.transform.position;

        Assert.Greater(after.x, before.x);
        Assert.AreEqual(before.y, after.y, 0.001f);

        yield return null;
    }

    [UnityTest]
    public IEnumerator Move_ShouldMoveEnemyUsingCurrentMoveSpeed()
    {
        CreateSpawnManager();
        CreatePlayer(new Vector3(10f, 0f, 0f));

        TestChargingEnemyMovement movement = CreateEnemy(10f);

        movement.CallStart();

        Vector3 before = movement.transform.position;

        movement.CallMove();

        Vector3 after = movement.transform.position;

        Assert.Greater(after.x, before.x);

        yield return null;
    }
}