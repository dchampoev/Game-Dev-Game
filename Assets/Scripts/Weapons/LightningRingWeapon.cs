using System.Collections.Generic;
using UnityEngine;

public class LightningRingWeapon : ProjectileWeapon
{
    List<EnemyStats> targetedEnemies = new List<EnemyStats>();

    protected override bool Attack(int attackCount = 1)
    {
        if (!currentStats.hitEffect)
        {
            Debug.LogWarning("Lightning Ring weapon has no projectile prefab.");
            currentCooldown = data.baseStats.cooldown;
            return false;
        }

        if (!CanAttack()) return false;

        if (currentCooldown <= 0f)
        {
            targetedEnemies = new List<EnemyStats>(FindObjectsByType<EnemyStats>(FindObjectsSortMode.None));
            currentCooldown += currentStats.cooldown;
            currentAttackCount = attackCount;
        }

        EnemyStats target = PickEnemy();
        if (target)
        {
            DamageArea(target.transform.position, currentStats.area, GetDamage());

            Instantiate(currentStats.hitEffect, target.transform.position, Quaternion.identity);
        }

        if (attackCount > 0)
        {
            currentAttackCount = attackCount - 1;
            currentAttackInterval = currentStats.projectileInterval;
        }

        return true;
    }

    EnemyStats PickEnemy()
    {
        EnemyStats target = null;
        while (!target && targetedEnemies.Count > 0)
        {
            int index = Random.Range(0, targetedEnemies.Count);
            target = targetedEnemies[index];
            if (!target)
            {
                targetedEnemies.RemoveAt(index);
                continue;
            }

            Renderer targetRenderer = target.GetComponent<Renderer>();
            if (!targetRenderer || !targetRenderer.isVisible)
            {
                targetedEnemies.RemoveAt(index);
                target = null;
                continue;
            }
        }

        targetedEnemies.Remove(target);
        return target;
    }

    void DamageArea(Vector2 positon, float radius, float damage)
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(positon, radius);
        foreach (Collider2D target in targets)
        {
            EnemyStats enemy = target.GetComponent<EnemyStats>();
            if (enemy)
            {
                enemy.TakeDamage(damage, transform.position);
            }
        }
    }
}
