using System;
using UnityEngine;

[CreateAssetMenu(fileName = "RingEventData", menuName = "2D Rogue-like/Event Data/Ring")]
public class RingEventData : EventData
{
    [Header("Mob Data")]
    public ParticleSystem spawnEffectPrefab;
    public Vector2 scale = new Vector2(1, 1);
    [Min(0)] public float spawnRadius = 10f, lifespan = 15f;

    public override bool Active(PlayerStats player = null)
    {
        if (player)
        {
            GameObject[] spawns = GetSpawns();
            float angleOffset = 2 * Mathf.PI / Mathf.Max(1, spawns.Length);
            float currentAngle = 0;
            foreach (GameObject gameObject in spawns)
            {
                Vector3 spawnPosition = player.transform.position + new Vector3(
                    spawnRadius * Mathf.Cos(currentAngle) * scale.x,
                    spawnRadius * Mathf.Sin(currentAngle) * scale.y
                );

                if (spawnEffectPrefab)
                {
                    Instantiate(spawnEffectPrefab, spawnPosition, Quaternion.identity);
                }

                GameObject spawned = Instantiate(gameObject, spawnPosition, Quaternion.identity);
                if (lifespan > 0) Destroy(spawned, lifespan);

                currentAngle += angleOffset;
            }
        }
        return false;
    }
}
