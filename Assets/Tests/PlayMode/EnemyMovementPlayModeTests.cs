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
        enemyStats.currentMoveSpeed = 5f;

        enemyObject.transform.position = Vector2.zero;

        movement.CallStart();

        Vector2 before = enemyObject.transform.position;

        movement.CallUpdate();

        Vector2 after = enemyObject.transform.position;

        Assert.AreNotEqual(before, after);

        yield return null;
    }
}