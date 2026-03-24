using UnityEngine;
using UnityEngine.TestTools;

[ExcludeFromCoverage]
public class PlayerCollector : MonoBehaviour
{
    PlayerStats player;
    CircleCollider2D playerCollector;
    public float pullSpeed;

    void Start()
    {
        player = FindAnyObjectByType<PlayerStats>();
        playerCollector = GetComponent<CircleCollider2D>();
    }

    void Update()
    {
        playerCollector.radius = player.CurrentMagnet;
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Pickup")) return;
        Rigidbody2D rigidbody = collision.gameObject.GetComponent<Rigidbody2D>();
        if (rigidbody != null)
        {
            Vector2 direction = ((Vector2)transform.position - rigidbody.position).normalized;
            rigidbody.linearVelocity = direction * pullSpeed;
        }
    }
}