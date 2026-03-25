using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TreasureChestPlayModeTests
{
    [UnityTest]
    public IEnumerator OnTriggerEnter2D_WhenPlayerTouchesChest_ShouldDestroyChest()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";

        player.AddComponent<BoxCollider2D>();

        Rigidbody2D playerRb = player.AddComponent<Rigidbody2D>();
        playerRb.gravityScale = 0f;
        playerRb.bodyType = RigidbodyType2D.Kinematic;

        PlayerInventory inventory = player.AddComponent<PlayerInventory>();
        inventory.weaponSlots = new List<PlayerInventory.Slot>();

        GameObject chestObject = new GameObject("Chest");
        BoxCollider2D chestCollider = chestObject.AddComponent<BoxCollider2D>();
        chestCollider.isTrigger = true;
        chestObject.AddComponent<TreasureChest>();

        chestObject.transform.position = Vector2.zero;
        player.transform.position = new Vector2(-2f, 0f);

        yield return new WaitForFixedUpdate();

        player.transform.position = Vector2.zero;

        yield return new WaitForFixedUpdate();
        yield return null;

        Assert.IsTrue(chestObject == null);

        if (player != null)
            Object.Destroy(player);
    }
}