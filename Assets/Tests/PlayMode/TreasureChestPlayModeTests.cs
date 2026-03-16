using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class TreasureChestPlayModeTests
{
    [UnityTest]
    public IEnumerator OnTriggerEnter2D_WhenPlayerTouchesChest_ShouldDestroyChest()
    {
        GameObject inventoryObject = new GameObject("Inventory");
        InventoryManager inventory = inventoryObject.AddComponent<InventoryManager>();

        inventory.weaponSlots = new List<WeaponController> { null, null, null, null, null, null };
        inventory.passiveItemSlots = new List<PassiveItem> { null, null, null, null, null, null };
        inventory.weaponUISlots = new List<Image>();
        inventory.passiveItemUISlots = new List<Image>();
        inventory.weaponEvolutions = new List<WeaponEvolutionBlueprint>();

        for (int i = 0; i < 6; i++)
        {
            inventory.weaponUISlots.Add(new GameObject($"WeaponUI{i}").AddComponent<Image>());
            inventory.passiveItemUISlots.Add(new GameObject($"PassiveUI{i}").AddComponent<Image>());
        }

        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.AddComponent<BoxCollider2D>();
        Rigidbody2D playerRb = player.AddComponent<Rigidbody2D>();
        playerRb.gravityScale = 0f;
        playerRb.bodyType = RigidbodyType2D.Kinematic;

        GameObject chestObject = new GameObject("Chest");
        chestObject.AddComponent<BoxCollider2D>().isTrigger = true;
        chestObject.AddComponent<TreasureChest>();

        chestObject.transform.position = Vector2.zero;
        player.transform.position = new Vector2(-2f, 0f);

        yield return new WaitForFixedUpdate();

        player.transform.position = Vector2.zero;

        yield return new WaitForFixedUpdate();
        yield return null;

        Assert.IsTrue(chestObject == null);

        Object.Destroy(inventoryObject);
        Object.Destroy(player);
    }
}