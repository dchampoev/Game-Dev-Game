using UnityEngine;

public class KnifeController : WeaponController
{

    protected override void Start()
    {
        base.Start();
    }

    protected override void Attack()
    {
        base.Attack();
        GameObject spawnedKnife = Instantiate(prefab);
        spawnedKnife.transform.position = transform.position; // Set the position of the spawned knife to be the same as the position of the player
        spawnedKnife.GetComponent<KnifeBehaviour>().DirectionChecker(playerMovement.lastMoveDirection);
    }
}
