using UnityEngine;

/// <summary>
/// Base script for all projectile behaviours[To be placed on a prefab of a weapon that is a projectile].
/// </summary>
public class ProjectileWeaponBehaviour : MonoBehaviour
{
    public WeaponScriptableObject weaponData;

    protected Vector3 travelDirection;
    public float lifetimeSeconds;

    protected float currentDamage;
    protected float currentSpeed;
    protected float currentCooldownDuration;
    protected int currentPierce;

    public void InitializeStats()
    {
        currentDamage = weaponData.Damage;
        currentSpeed = weaponData.Speed;
        currentCooldownDuration = weaponData.CooldownDuration;
        currentPierce = weaponData.Pierce;
    }


    void Awake()
    {
        if (weaponData == null) return;

        InitializeStats();
    }

    public float GetCurrentDamage()
    {
        PlayerStats player = FindAnyObjectByType<PlayerStats>();
        float might = player != null ? player.CurrentMight : 1f;
        return currentDamage * might;
    }

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

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyStats enemy = collision.GetComponent<EnemyStats>();
            if (enemy != null)
            {
                enemy.TakeDamage(GetCurrentDamage(),transform.position);
                reducePierce();
            }
        }
        else if (collision.CompareTag("Prop"))
        {
            if (collision.gameObject.TryGetComponent(out BreakableProps breakable))
            {
                breakable.TakeDamage(GetCurrentDamage());
                reducePierce();
            }
        }
    }

    void reducePierce()
    {
        currentPierce--;
        if (currentPierce <= 0)
        {
            Destroy(gameObject);
        }
    }
}
