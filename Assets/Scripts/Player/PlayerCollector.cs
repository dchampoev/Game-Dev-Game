using UnityEngine;
using UnityEngine.TestTools;

[ExcludeFromCoverage]
[RequireComponent(typeof(CircleCollider2D))]
public class PlayerCollector : MonoBehaviour
{
    PlayerStats player;
    CircleCollider2D detector;
    public float pullSpeed;

    void Start()
    {
       player = GetComponentInParent<PlayerStats>();
    }

    public void SetRadius(float radius)
    {
        if (!detector) detector = GetComponent<CircleCollider2D>();
        detector.radius = radius;
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out Pickup pickup)){
            pickup.Collect(player, pullSpeed);
        }
    }
}