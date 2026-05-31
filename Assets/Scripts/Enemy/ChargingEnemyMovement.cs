using UnityEngine;

public class ChargingEnemyMovement : EnemyMovement
{
    Vector2 chargeDirection;

    protected override void Start()
    {
        base.Start();
        chargeDirection = ((Vector2)player.position - rigidBody.position).normalized;
    }

    protected override void Move(float deltaTime)
    {
        if (rigidBody)
        {
            rigidBody.linearVelocity = chargeDirection * stats.Actual.moveSpeed;
        }
        else
        {
            transform.position += (Vector3)(chargeDirection * stats.Actual.moveSpeed * deltaTime);
        }
    }
}
