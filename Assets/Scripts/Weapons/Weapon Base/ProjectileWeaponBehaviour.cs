using UnityEngine;

/// <summary>
/// Base script for all projectile behaviours[To be placed on a prefab of a weapon that is a projectile].
/// </summary>
public class ProjectileWeaponBehaviour : MonoBehaviour
{
    protected Vector3 direction;
    public float destroyAfterSeconds;

    protected virtual void Start()
    {
        Destroy(gameObject, destroyAfterSeconds);
    }

    public void DirectionChecker(Vector3 dir)
    {
        direction = dir.normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // Get the angle of the direction vector in degrees
        angle -= 45f;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
