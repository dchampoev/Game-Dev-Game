using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderLoopWeapon : ProjectileWeapon
{
    List<EnemyStats> targetedEnemies = new List<EnemyStats>();

    protected override bool Attack(int attackCount = 1)
    {
        if (!currentStats.hitEffect)
        {
            Debug.LogWarning("Thunder Loop weapon has no hit effect assigned.");
            ActivateCooldown(true);
            return false;
        }

        if (!CanAttack())
            return false;

        if (currentCooldown <= 0f)
        {
            targetedEnemies = new List<EnemyStats>(FindObjectsByType<EnemyStats>(FindObjectsSortMode.None));
            ActivateCooldown();
            currentAttackCount = attackCount;
        }

        EnemyStats target = PickEnemy();
        if (target)
        {
            StartCoroutine(Strike(target));
        }

        if (currentStats.procEffect)
        {
            Destroy(Instantiate(currentStats.procEffect, owner.transform), 5f);
        }

        if (attackCount > 0)
        {
            currentAttackCount = attackCount - 1;
            currentAttackInterval = currentStats.projectileInterval;
        }

        return true;
    }

    IEnumerator Strike(EnemyStats target)
    {
        if (!target)
            yield break;

        Vector2 strikePosition = target.transform.position;

        DamageArea(strikePosition, GetArea(), GetDamage());

        float randomAngle = Random.value < 0.5f ? 45f : -45f;

        ParticleSystem thunderLoopVFX = Instantiate(currentStats.hitEffect, strikePosition, Quaternion.Euler(0f, 0f, randomAngle));
        thunderLoopVFX.transform.localScale = new Vector3(GetArea(), GetArea(), GetArea());

        yield return new WaitForSeconds(0.9f);

        if (thunderLoopVFX)
            thunderLoopVFX.transform.rotation = Quaternion.identity;

        yield return new WaitForSeconds(0.1f);

        DamageArea(strikePosition, GetArea(), GetDamage());

        if (thunderLoopVFX)
            Destroy(thunderLoopVFX.gameObject, 5f);
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
                ApplyBuffs(enemy);
            }
        }
    }
}
