using UnityEngine;

public class BloodyTearProjectile : Projectile
{
    protected override void Start()
    {
        base.Start();
    }

    public override void AcquireAutoAimFacing()
    {
        base.AcquireAutoAimFacing();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);

        if (weapon.criticalHit)
        {
            weapon.Owner.RestoreHealth(16);
        }
    }
}