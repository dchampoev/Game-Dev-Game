using UnityEngine;

public class WhipWeapon : ProjectileWeapon
{
    int currentSpawnCount;
    float currentSpawnYOffset;

    protected override bool Attack(int attackCount = 1)
    {
        if (!currentStats.projectilePrefab)
        {
            Debug.LogWarning("Whip weapon has no projectile prefab.");
            currentCooldown = data.baseStats.cooldown;
            return false;
        }

        if (!CanAttack()) return false;

        if (currentCooldown <= 0f)
        {
            currentSpawnCount = 0;
            currentSpawnYOffset = 0f;
        }

        float spawnDirection = Mathf.Sign(movement.lastMoveDirection.x) * (currentSpawnCount % 2 == 0 ? 1 : -1);
        Vector2 spawnOffset = new Vector2(
            spawnDirection * Random.Range(currentStats.spawnVariance.xMin, currentStats.spawnVariance.xMax),
            currentSpawnYOffset
        );

        Projectile prefab = Instantiate(
            currentStats.projectilePrefab,
            owner.transform.position + (Vector3)spawnOffset,
            Quaternion.identity
        );
        prefab.owner = owner;

        if (spawnDirection < 0)
        {
            prefab.transform.localScale = new Vector3(
                -Mathf.Abs(prefab.transform.localScale.x), prefab.transform.localScale.y, prefab.transform.localScale.z
            );
        }

        prefab.weapon = this;
        currentCooldown = data.baseStats.cooldown;
        attackCount--;

        currentSpawnCount++;
        if (currentSpawnCount > 1 && currentSpawnCount % 2 == 0)
        {
            currentSpawnYOffset += 1f;
        }

        if (attackCount > 0)
        {
            currentAttackCount = attackCount;
            currentAttackInterval = data.baseStats.projectileInterval;
        }

        return true;
    }
}
