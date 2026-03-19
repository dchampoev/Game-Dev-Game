using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Base script for all melee weapon behaviours[To be placed on a prefab of a weapon that is a melee].
/// </summary>

public class MeleeWeaponBehaviour : MonoBehaviour
{
    public WeaponScriptableObject weaponData;
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

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyStats enemy = collision.GetComponent<EnemyStats>();
            if (enemy != null)
            {
                enemy.TakeDamage(GetCurrentDamage(), transform.position);
            }
        }
        else if (collision.CompareTag("Prop"))
        {
            if (collision.gameObject.TryGetComponent(out BreakableProps breakable))
            {
                breakable.TakeDamage(GetCurrentDamage());
            }
        }
    }
}
