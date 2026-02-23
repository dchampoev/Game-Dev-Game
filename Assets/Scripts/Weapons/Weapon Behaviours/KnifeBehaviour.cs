using UnityEngine;

public class KnifeBehaviour : ProjectileWeaponBehaviour
{
    KnifeController knifeController;

    protected override void Start()
    {
        base.Start();
        knifeController = FindAnyObjectByType<KnifeController>();
    }

    void Update()
    {
        transform.position += direction*knifeController.speed*Time.deltaTime; // Set the movement of the knife
    }
}
