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

    [UnityTest]
    public IEnumerator Update_ShouldMoveEnemyTowardsPlayer()
    {
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

        Object.Destroy(enemyObject);
        Object.Destroy(playerObject);

        yield return null;
    }
}