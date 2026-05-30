using UnityEngine;

public class KingBibleWeapon : ProjectileWeapon
{
    public void SpawnRing(PlayerStats player = null)
    {
        if (player)
        {
            float angleOffset = 2 * Mathf.PI / Mathf.Max(1, currentStats.number + owner.Stats.amount);
            float currentAngle = 0;
            for (int i = 0; i < currentStats.number + owner.Stats.amount; i++)
            {
                Vector3 spawnPosition = player.transform.position + new Vector3(
                    GetArea() * Mathf.Cos(currentAngle),
                    GetArea() * Mathf.Sin(currentAngle)
                );

                Projectile prefab = Instantiate(currentStats.projectilePrefab, spawnPosition, Quaternion.identity, owner.transform);

                prefab.owner = owner;
                currentAngle += angleOffset;
                prefab.weapon = this;
            }
        }
    }
    protected override bool Attack(int attackCount = 1)
    {
        if (!currentStats.projectilePrefab)
        {
            Debug.LogWarning(string.Format("Projectile prefab has not been set for {0}", name));
            ActivateCooldown(true);
            return false;
        }

        if (!CanAttack()) return false;

        SpawnRing(owner);

        ActivateCooldown(true);

        attackCount--;

        if (attackCount > 0)
        {
            currentAttackCount = attackCount;
            currentAttackInterval = ((WeaponData)data).baseStats.projectileInterval;
        }


        return true;
    }

    public override bool ActivateCooldown(bool strict = false)
    {
        if (strict && currentCooldown > 0) return false;

        float actualCooldown = (currentStats.lifespan + currentStats.cooldown) * Owner.Stats.cooldown;

        currentCooldown = Mathf.Min(actualCooldown, currentCooldown + actualCooldown);
        return true;
    }

    public override bool DoLevelUp(bool updateUI = true)
    {
        if (!base.DoLevelUp(updateUI)) return false;

        SpawnRing(owner);
        ActivateCooldown(false);
        return true;
    }
}
