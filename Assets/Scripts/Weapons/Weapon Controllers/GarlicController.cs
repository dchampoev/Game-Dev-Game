using UnityEngine;

public class GarlicController : WeaponController
{



    protected override void Start()
    {
        base.Start();
    }


    protected override void Attack()
    {
        base.Attack();
        GameObject spawnedGarlic = Instantiate(prefab);
        spawnedGarlic.transform.position = transform.position; //Assign the position of the garlic to the position of the player
        spawnedGarlic.transform.parent = transform; //Make the garlic a child of the player so it moves with the player
    }
}
