using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

public class TreasureChestTests
{
    private class TestTreasureChest : TreasureChest
    {
        public void CallStart()
        {
            typeof(TreasureChest)
                .GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(this, null);
        }
    }

    [Test]
    public void OpenTreasureChest_WhenNoPossibleEvolutions_ShouldNotThrow()
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

        GameObject chestObject = new GameObject("Chest");
        TestTreasureChest chest = chestObject.AddComponent<TestTreasureChest>();

        chest.CallStart();

        Assert.DoesNotThrow(() => chest.OpenTreasureChest());

        Object.DestroyImmediate(chestObject);
        Object.DestroyImmediate(inventoryObject);
    }
}