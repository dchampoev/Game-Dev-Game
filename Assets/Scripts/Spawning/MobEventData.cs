using UnityEngine;

[CreateAssetMenu(fileName = "MobEventData", menuName = "2D Rogue-like/Event Data/Mob")]
public class MobEventData : EventData
{
    [Header("Mob Data")]
    [Range(0f, 360f)] public float possibleAngles = 360f;
    [Min(0)] public float spawnRadius = 2f, spawnDistance = 20f;

    public override bool Active(PlayerStats player = null)
    {
        if (player)
        {
            float randomAngle = Random.Range(0f, possibleAngles) * Mathf.Deg2Rad;
            foreach (GameObject gameObject in GetSpawns())
            {
                Instantiate(gameObject, player.transform.position + new Vector3(
                    (spawnDistance + Random.Range(-spawnRadius, spawnRadius)) * Mathf.Cos(randomAngle),
                    (spawnDistance + Random.Range(-spawnRadius, spawnRadius)) * Mathf.Sin(randomAngle)
                    ), Quaternion.identity);
            }
        }
        return false;
    }
}
