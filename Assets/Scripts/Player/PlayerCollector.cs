using Terresquall;
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
        if (!detector)
            detector = GetComponent<CircleCollider2D>();
        detector.radius = radius;
    }
    public float GetCoins() { return coins; }

    public float AddCoins(float amount)
    {
        coins += amount;
        onCoinCollected?.Invoke();
        return coins;
    }

    public void SaveCoinsToStash(bool async)
    {
        if (Mathf.Approximately(coins, 0f))
            return;

        SaveManager saveManager = SaveManager.Instance;
        saveManager.AddCoins(coins);
        ResetRunCoins();

        if (async)
            Bench.SaveGameAsync();
        else
            Bench.SaveGame();
    }

    public void SaveCoinsToStash()
    {
        SaveCoinsToStash(false);
    }

    public void ResetRunCoins()
    {
        coins = 0;
        onCoinCollected?.Invoke();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Pickup pickup))
        {
            pickup.Collect(player, pullSpeed);
        }
    }
}
