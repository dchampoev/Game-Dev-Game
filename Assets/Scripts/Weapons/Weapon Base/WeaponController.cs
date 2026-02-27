using UnityEngine;

/// <summary>
/// Base script for all weapon controllers.
/// </summary>

public class WeaponController : MonoBehaviour
{
    [Header("Weapon Stats")]
    public WeaponScriptableObject weaponData;
    float currentCooldown;

    protected PlayerMovement playerMovement;


    protected virtual void Start()
    {
        playerMovement = FindAnyObjectByType<PlayerMovement>();
        currentCooldown = weaponData.CooldownDuration;
    }

    protected virtual void Update()
    {
        currentCooldown -= Time.deltaTime;
        if(currentCooldown <= 0f)
        {
            Attack();
        }
    }

    protected virtual void Attack()
    {
        currentCooldown = weaponData.CooldownDuration;
    }
}
