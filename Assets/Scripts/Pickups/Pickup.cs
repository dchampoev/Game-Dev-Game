using UnityEngine;

public class Pickup : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            if(TryGetComponent(out ICollectable collectable))
            {
                collectable.Collect();
            }
            Destroy(gameObject);
        }
    }
}
