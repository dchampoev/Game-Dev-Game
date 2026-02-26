using UnityEngine;

/// <summary>
/// Base script for all projectile behaviours[To be placed on a prefab of a weapon that is a projectile].
/// </summary>
public class ProjectileWeaponBehaviour : MonoBehaviour
{
    protected Vector3 travelDirection;
    public float lifetimeSeconds;

    protected virtual void Start()
    {
        Destroy(gameObject, lifetimeSeconds);
    }

    public void DirectionChecker(Vector3 direction)
    {
        travelDirection = direction.normalized;

        float angle = Mathf.Atan2(travelDirection.y, travelDirection.x) * Mathf.Rad2Deg;
        angle -= 45f;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
