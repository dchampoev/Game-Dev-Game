using UnityEngine;

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
        playerCollector.radius = player.currentMagnet;
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        Rigidbody2D rigidbody = collision.gameObject.GetComponent<Rigidbody2D>();
        if (rigidbody != null)
        {
            Vector2 currentPosition = collision.transform.position;
            Vector2 targetPosition = transform.position;
            float step = pullSpeed * Time.deltaTime;

            Vector2 newPosition = Vector2.MoveTowards(currentPosition, targetPosition, step);
            rigidbody.MovePosition(newPosition);
        }
    }
}