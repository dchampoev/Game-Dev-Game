public class CoinPickup : Pickup
{
    PlayerCollector collector;
    public int coins = 1;

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (target != null)
        {
            collector = target.GetComponentInChildren<PlayerCollector>();
            if (collector != null)
                collector.AddCoins(coins);
        }
    }
}
