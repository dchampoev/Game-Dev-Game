using Unity.VisualScripting;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public EnemyScriptableObject enemyData;

    //Current stats
    [HideInInspector]
    public float currentMoveSpeed;
    [HideInInspector]
    public float currentHealth;
    [HideInInspector]
    public float currentDamage;

    public float despawnDistance = 20f; // Distance at which the enemy will be relocated near the player
    Transform player;

    void Awake()
    {
        currentMoveSpeed = enemyData.MoveSpeed;
        currentHealth = enemyData.MaxHealth;
        currentDamage = enemyData.Damage;
    }

    void Start()
    {
        player = FindAnyObjectByType<PlayerStats>().transform;
    }

    void Update()
    {
        if (Vector2.Distance(transform.position, player.position) >= despawnDistance)
        {
            RelocateNearPlayer();
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Kill();
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerStats player = collision.gameObject.GetComponent<PlayerStats>();
            if (player != null)
            {
                player.TakeDamage(currentDamage);
            }
        }
    }

    private void Kill()
    {
        EnemySpawner spawner = FindAnyObjectByType<EnemySpawner>();
        spawner.OnEnemyKilled();
        Destroy(gameObject);
    }

    void RelocateNearPlayer()
    {
        EnemySpawner spawner = FindAnyObjectByType<EnemySpawner>();
        transform.position = player.position + spawner.relativeSpawnPoints[UnityEngine.Random.Range(0, spawner.relativeSpawnPoints.Count)].position;
    }
}
