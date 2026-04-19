using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An aura is a damage-over-time effect that applies to a specific area in timed intervals.
/// </summary>
public class Aura : WeaponEffect
{
    Dictionary<EnemyStats, float> affectedEnemies = new Dictionary<EnemyStats, float>();
    List<EnemyStats> targetsToUnaffect = new List<EnemyStats>();

    void Update()
    {
        Dictionary<EnemyStats, float> affectedEnemiesCopy = new Dictionary<EnemyStats, float>(affectedEnemies);
        foreach (KeyValuePair<EnemyStats, float> pair in affectedEnemiesCopy)
        {
            affectedEnemies[pair.Key] -= Time.deltaTime;
            if (pair.Value <= 0)
            {
                if (targetsToUnaffect.Contains(pair.Key))
                {
                    affectedEnemies.Remove(pair.Key);
                    targetsToUnaffect.Remove(pair.Key);
                }
                else
                {
                    Weapon.Stats stats = weapon.GetStats();
                    affectedEnemies[pair.Key] = stats.cooldown * Owner.Stats.cooldown;
                    pair.Key.TakeDamage(GetDamage(), transform.position, stats.knockback);

                    if (stats.hitEffect)
                    {
                        Destroy(Instantiate(stats.hitEffect, pair.Key.transform.position, Quaternion.identity), 5f);
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
        if(collision.TryGetComponent(out EnemyStats enemy))
        {
            if (affectedEnemies.ContainsKey(enemy))
            {
                targetsToUnaffect.Add(enemy);
            }
        }
    }
}
