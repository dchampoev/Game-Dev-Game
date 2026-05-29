using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CoinPickupPlayModeTests
{
    class TestCoinPickup : CoinPickup
    {
        public void SetTarget(PlayerStats player)
        {
            typeof(Pickup)
                .GetField("target", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(this, player);
        }

        public void TriggerDestroy()
        {
            OnDestroy();
        }
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        foreach (GameObject obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.Destroy(obj);
        }

        yield return null;
    }

    [UnityTest]
    public IEnumerator OnDestroy_WhenCollectedByPlayer_ShouldAddCoinsToCollector()
    {
        GameObject playerObject = new GameObject("Player");
        playerObject.AddComponent<PlayerInventory>();
        PlayerStats player = playerObject.AddComponent<PlayerStats>();

        GameObject collectorObject = new GameObject("Collector");
        collectorObject.transform.SetParent(playerObject.transform);
        PlayerCollector collector = collectorObject.AddComponent<PlayerCollector>();

        TestCoinPickup pickup = new GameObject("Coin Pickup").AddComponent<TestCoinPickup>();
        pickup.coins = 7;
        pickup.SetTarget(player);

        pickup.TriggerDestroy();

        Assert.AreEqual(7f, collector.GetCoins());
        yield return null;
    }
}
