using UnityEngine;

[System.Obsolete("KnifeBehaviour is no longer used.")]
public class KnifeBehaviour : ProjectileWeaponBehaviour
{
    protected override void Start()
    {
        base.Start();
    }

    void Update()
    {
        transform.position += travelDirection * currentSpeed * Time.deltaTime;
    }
}
