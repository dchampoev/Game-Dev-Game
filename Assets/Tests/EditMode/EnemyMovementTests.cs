using NUnit.Framework;
using UnityEngine;

public class EnemyMovementTests
{
    class TestEnemyMovement : EnemyMovement
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

    [Test]
    public void Update_ShouldMoveEnemyTowardsPlayer()
    {
        // Player
        GameObject playerObject = new GameObject("Player");
        PlayerMovement playerMovement = playerObject.AddComponent<PlayerMovement>();
        playerObject.transform.position = new Vector2(10, 0);

        // Enemy
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

        Object.DestroyImmediate(enemyObject);
        Object.DestroyImmediate(playerObject);
    }
}