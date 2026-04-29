using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class ItemCanEvolveTests
{
    private class TestItem : Item
    {
        public void SetInventory(PlayerInventory inv) => inventory = inv;
        public void SetEvolutionData(ItemData.Evolution[] data) => evolutionData = data;
    }

    private class TestWeapon : Weapon
    {
    }

    private WeaponData CreateWeaponData(string name)
    {
        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
        data.name = name;
        data.maxLevel = 5;
        data.baseStats = new Weapon.Stats();
        data.linearGrowth = new Weapon.Stats[0];
        data.randomGrowth = new Weapon.Stats[0];
        return data;
    }

    private PlayerInventory CreateInventory()
    {
        GameObject go = new GameObject();
        PlayerInventory inv = go.AddComponent<PlayerInventory>();
        inv.weaponSlots = new List<PlayerInventory.Slot>();
        inv.passiveSlots = new List<PlayerInventory.Slot>();
        return inv;
    }

    [Test]
    public void CanEvolve_WhenOneValidEvolution_ShouldReturnOnlyThatOne()
    {
        PlayerInventory inventory = CreateInventory();

        GameObject itemGO = new GameObject();
        TestItem item = itemGO.AddComponent<TestItem>();
        item.currentLevel = 2;
        item.SetInventory(inventory);

        WeaponData catalyst = CreateWeaponData("Catalyst");
        WeaponData outcome = CreateWeaponData("Outcome");

        GameObject catalystGO = new GameObject();
        TestWeapon weapon = catalystGO.AddComponent<TestWeapon>();
        weapon.data = catalyst;
        weapon.currentLevel = 1;

        inventory.weaponSlots.Add(new PlayerInventory.Slot
        {
            item = weapon,
        });

        ItemData.Evolution valid = new ItemData.Evolution
        {
            evolutionLevel = 2,
            catalysts = new[]
            {
                new ItemData.Evolution.Config { itemType = catalyst, level = 1 }
            },
            outcome = new ItemData.Evolution.Config { itemType = outcome }
        };

        ItemData.Evolution invalid = new ItemData.Evolution
        {
            evolutionLevel = 5
        };

        item.SetEvolutionData(new[] { valid, invalid });

        var result = item.CanEvolve();

        Assert.AreEqual(1, result.Length);
    }
}