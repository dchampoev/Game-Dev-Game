using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PickupPlayModeTests
{
    [UnityTest]
    public IEnumerator OnTriggerEnter_PlayerCollectsPickup_ObjectDestroyed()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";

        BoxCollider2D playerCollider = player.AddComponent<BoxCollider2D>();
        Rigidbody2D playerRb = player.AddComponent<Rigidbody2D>();
        playerRb.gravityScale = 0f;
        playerRb.bodyType = RigidbodyType2D.Kinematic;

        GameObject pickupObject = new GameObject("Pickup");
        BoxCollider2D pickupCollider = pickupObject.AddComponent<BoxCollider2D>();
        pickupCollider.isTrigger = true;
        pickupObject.AddComponent<Pickup>();

        pickupObject.transform.position = Vector2.zero;
        player.transform.position = new Vector2(-2f, 0f);

        yield return new WaitForFixedUpdate();

        player.transform.position = Vector2.zero;

        yield return new WaitForFixedUpdate();
        yield return null;

        Assert.IsTrue(pickupObject == null);

        Object.Destroy(player);
    }

    [UnityTest]
    public IEnumerator Pickup_ShouldCallCollect_WhenCollectableExists()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.AddComponent<BoxCollider2D>();
        player.AddComponent<Rigidbody2D>().gravityScale = 0;

        GameObject pickupObject = new GameObject("Pickup");
        pickupObject.AddComponent<BoxCollider2D>().isTrigger = true;
        pickupObject.AddComponent<Pickup>();

        var collectable = pickupObject.AddComponent<TestCollectable>();

        player.transform.position = Vector2.zero;
        pickupObject.transform.position = Vector2.zero;

        yield return new WaitForFixedUpdate();

        Assert.IsTrue(collectable.wasCollected);

        Object.Destroy(player);
    }
}