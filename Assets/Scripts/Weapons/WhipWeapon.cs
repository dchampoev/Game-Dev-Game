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
            ActivateCooldown(true);
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

        if (currentStats.procEffect)
        {
            Destroy(Instantiate(currentStats.procEffect, owner.transform), 5f);
        }

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
        ActivateCooldown(true);
        attackCount--;

        currentSpawnCount++;
        if (currentSpawnCount > 1 && currentSpawnCount % 2 == 0)
        {
            currentSpawnYOffset += 1f;
        }

        if (attackCount > 0)
        {
            currentAttackCount = attackCount;
            currentAttackInterval = ((WeaponData)data).baseStats.projectileInterval;
        }

        return true;
    }
}
