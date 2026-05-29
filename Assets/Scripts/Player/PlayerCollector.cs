using UnityEngine;
using UnityEngine.TestTools;

[ExcludeFromCoverage]
[RequireComponent(typeof(CircleCollider2D))]
public class PlayerCollector : MonoBehaviour
{
    PlayerStats player;
    CircleCollider2D detector;
    public float pullSpeed = 10;

    public delegate void OnCoinCollected();
    public OnCoinCollected onCoinCollected;

    float coins;

    void Start()
    {
        player = GetComponentInParent<PlayerStats>();
        coins = 0;
    }

    public void SetRadius(float radius)
    {
        if (!detector) detector = GetComponent<CircleCollider2D>();
        detector.radius = radius;
    }
    public float GetCoins() { return coins; }

    public float AddCoins(float amount)
    {
        coins += amount;
        onCoinCollected?.Invoke();
        return coins;
    }

    public void SaveCoinsToStash()
    {
        if (Mathf.Approximately(coins, 0f)) return;

        SaveManager.LastLoadedGameData.coins += coins;
        coins = 0;
        SaveManager.Save();
        onCoinCollected?.Invoke();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out Pickup pickup)){
            pickup.Collect(player, pullSpeed);
        }
    }
}
