using UnityEngine;

public class EnemyMovement : Sortable
{
    static PlayerMovement[] cachedPlayers;

    protected EnemyStats stats;
    protected Transform player;
    protected Rigidbody2D rigidBody;

    protected Vector2 knockbackVelocity;
    protected float knockbackDuration;

    public enum OutOfFrameAction { none, respawnAtEdge, despawn }
    public OutOfFrameAction outOfFrameAction = OutOfFrameAction.respawnAtEdge;

    [System.Flags]
    public enum KnockbackVariance { duration = 1, velocity = 2 }
    public KnockbackVariance knockbackVariance = KnockbackVariance.velocity;

    protected bool spawnedOutOfFrame = false;

    protected virtual void Awake()
    {
        CacheComponents();
    }

    protected override void Start()
    {
        base.Start();

        CacheComponents();
        spawnedOutOfFrame = !SpawnManager.IsWithinBoundaries(transform);

        player = GetRandomPlayer();
    }

    void CacheComponents()
    {
        if (!rigidBody)
            rigidBody = GetComponent<Rigidbody2D>();
        if (!stats)
            stats = GetComponent<EnemyStats>();
    }

    protected virtual void Update()
    {
        if (!rigidBody)
        {
            TickMovement(Time.deltaTime);
        }
    }

    protected virtual void FixedUpdate()
    {
        if (rigidBody)
        {
            TickMovement(Time.fixedDeltaTime);
        }
    }

    void TickMovement(float deltaTime)
    {
        if (knockbackDuration > 0)
        {
            if (rigidBody)
                rigidBody.linearVelocity = knockbackVelocity;
            else
                transform.position += (Vector3)(knockbackVelocity * deltaTime);

            knockbackDuration -= deltaTime;
        }
        else
        {
            Move(deltaTime);
            HandleOutOfFrameAction();
        }
    }

    protected virtual void HandleOutOfFrameAction()
    {
        if (!SpawnManager.IsWithinBoundaries(transform))
        {
            switch (outOfFrameAction)
            {
                case OutOfFrameAction.none:
                default:
                    break;
                case OutOfFrameAction.respawnAtEdge:
                    transform.position = SpawnManager.GeneratePosition();
                    break;
                case OutOfFrameAction.despawn:
                    if (!spawnedOutOfFrame)
                    {
                        Destroy(gameObject);
                    }
                    break;
            }
        }
        else
            spawnedOutOfFrame = false;
    }

    public virtual void Knockback(Vector2 velocity, float duration)
    {
        CacheComponents();
        if (!stats)
            return;

        if (knockbackDuration > 0)
            return;

        if (knockbackVariance == 0)
            return;

        float pow = 1;
        bool reducesVelocity = (knockbackVariance & KnockbackVariance.velocity) > 0,
             reducesDuration = (knockbackVariance & KnockbackVariance.duration) > 0;

        if (reducesVelocity && reducesDuration)
            pow = 0.5f;

        knockbackVelocity = velocity * (reducesVelocity ? Mathf.Pow(stats.Actual.knockbackMultiplier, pow) : 1);
        knockbackDuration = duration * (reducesDuration ? Mathf.Pow(stats.Actual.knockbackMultiplier, pow) : 1);
    }

    public virtual void Move()
    {
        Move(Time.deltaTime);
    }

    protected virtual void Move(float deltaTime)
    {
        CacheComponents();
        if (!stats)
            return;

        if (!player)
            player = GetRandomPlayer();
        if (!player)
            return;

        if (rigidBody)
        {
            Vector2 direction = ((Vector2)player.position - rigidBody.position).normalized;
            rigidBody.linearVelocity = direction * stats.Actual.moveSpeed;
        }
        else
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.transform.position,
                stats.Actual.moveSpeed * deltaTime
            );
        }
    }

    static Transform GetRandomPlayer()
    {
        if (!HasCachedPlayer())
            cachedPlayers = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);

        if (cachedPlayers == null || cachedPlayers.Length == 0)
            return null;

        int startIndex = Random.Range(0, cachedPlayers.Length);
        for (int i = 0; i < cachedPlayers.Length; i++)
        {
            PlayerMovement candidate = cachedPlayers[(startIndex + i) % cachedPlayers.Length];
            if (candidate)
                return candidate.transform;
        }

        cachedPlayers = null;
        return null;
    }

    static bool HasCachedPlayer()
    {
        if (cachedPlayers == null || cachedPlayers.Length == 0)
            return false;

        foreach (PlayerMovement playerMovement in cachedPlayers)
        {
            if (playerMovement)
                return true;
        }

        return false;
    }
}
