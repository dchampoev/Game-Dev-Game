using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An aura is a damage-over-time effect that applies to a specific area in timed intervals.
/// </summary>
public class Aura : WeaponEffect
{
    Dictionary<EnemyStats, float> affectedEnemies = new Dictionary<EnemyStats, float>();
    List<EnemyStats> targetsToUnaffect = new List<EnemyStats>();
    readonly List<EnemyStats> affectedEnemiesSnapshot = new List<EnemyStats>();

    void Update()
    {
        affectedEnemiesSnapshot.Clear();
        foreach (EnemyStats enemy in affectedEnemies.Keys)
        {
            affectedEnemiesSnapshot.Add(enemy);
        }

        foreach (EnemyStats enemy in affectedEnemiesSnapshot)
        {
            if (!enemy)
            {
                affectedEnemies.Remove(enemy);
                targetsToUnaffect.Remove(enemy);
                continue;
            }

            float remainingTime = affectedEnemies[enemy];
            affectedEnemies[enemy] = remainingTime - Time.deltaTime;

            if (remainingTime <= 0)
            {
                if (targetsToUnaffect.Contains(enemy))
                {
                    affectedEnemies.Remove(enemy);
                    targetsToUnaffect.Remove(enemy);
                }
                else
                {
                    Weapon.Stats stats = weapon.GetStats();
                    affectedEnemies[enemy] = stats.cooldown * Owner.Stats.cooldown;
                    enemy.TakeDamage(GetDamage(), transform.position, stats.knockback);

                    weapon.ApplyBuffs(enemy);

                    if (stats.hitEffect)
                    {
                        Destroy(Instantiate(stats.hitEffect, enemy.transform.position, Quaternion.identity), 5f);
                    }
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out EnemyStats enemy))
        {
            if (!affectedEnemies.ContainsKey(enemy))
            {
                affectedEnemies.Add(enemy, 0);
            }
            else
            {
                if (targetsToUnaffect.Contains(enemy))
                {
                    targetsToUnaffect.Remove(enemy);
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out EnemyStats enemy))
        {
            if (affectedEnemies.ContainsKey(enemy))
            {
                if (!targetsToUnaffect.Contains(enemy))
                    targetsToUnaffect.Add(enemy);
            }
        }
    }
}
