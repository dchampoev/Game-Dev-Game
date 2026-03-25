using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class ItemAttemptEvolutionPlayModeTests
{
    private class TestItem : Item
    {
        public void SetInventory(PlayerInventory inv) => inventory = inv;
    }

    private class TestWeapon : Weapon
    {
    }

    private WeaponData CreateWeaponData(string name)
    {
        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
        data.name = name;
        data.maxLevel = 5;
        data.baseStats = new Weapon.Stats
        {
            name = name
        };
        data.linearGrowth = new Weapon.Stats[0];
        data.randomGrowth = new Weapon.Stats[0];
        return data;
    }

    private PassiveData CreatePassiveData(string name)
    {
        PassiveData data = ScriptableObject.CreateInstance<PassiveData>();
        data.name = name;
        data.maxLevel = 5;
        data.baseStats = new Passive.Modifier
        {
            name = name,
            description = "Outcome"
        };
        data.growth = new Passive.Modifier[0];
        return data;
    }

    private PlayerInventory.Slot CreateSlot(Item item)
    {
        GameObject imageObject = new GameObject("Image");
        Image image = imageObject.AddComponent<Image>();

        return new PlayerInventory.Slot
        {
            item = item,
            image = image
        };
    }

    private void SetPrivateField(object target, string fieldName, object value)
    {
        target.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(target, value);
    }

    [UnityTest]
    public IEnumerator AttemptEvolution_WhenValid_ShouldRemoveCatalystAndAddOutcome()
    {
        GameObject playerObject = new GameObject("Player");

        PlayerMovement movement = playerObject.AddComponent<PlayerMovement>();
        movement.enabled = false;

        PlayerStats stats = playerObject.AddComponent<PlayerStats>();
        stats.enabled = false;

        PlayerInventory inventory = playerObject.AddComponent<PlayerInventory>();
        inventory.weaponSlots = new List<PlayerInventory.Slot>();
        inventory.passiveSlots = new List<PlayerInventory.Slot>();

        SetPrivateField(stats, "inventory", inventory);

        yield return null;

        GameObject itemGO = new GameObject("Item");
        TestItem item = itemGO.AddComponent<TestItem>();
        item.currentLevel = 2;
        item.SetInventory(inventory);

        WeaponData catalystData = CreateWeaponData("Catalyst");
        PassiveData outcomeData = CreatePassiveData("Outcome");

        GameObject catalystGO = new GameObject("CatalystWeapon");
        TestWeapon catalystWeapon = catalystGO.AddComponent<TestWeapon>();
        catalystWeapon.data = catalystData;
        catalystWeapon.currentLevel = 1;

        inventory.weaponSlots.Add(CreateSlot(catalystWeapon));
        inventory.passiveSlots.Add(new PlayerInventory.Slot
        {
            item = null,
            image = new GameObject("EmptyPassiveSlotImage").AddComponent<Image>()
        });

        ItemData.Evolution evolution = new ItemData.Evolution
        {
            evolutionLevel = 2,
            consumes = ItemData.Evolution.Consumption.weapons,
            catalysts = new[]
            {
                new ItemData.Evolution.Config
                {
                    itemType = catalystData,
                    level = 1
                }
            },
            outcome = new ItemData.Evolution.Config
            {
                itemType = outcomeData,
                level = 1
            }
        };

        bool result = item.AttemptEvolution(evolution);

        yield return null;

        Assert.IsTrue(result);
        Assert.IsNull(inventory.Get(catalystData));
        Assert.IsNotNull(inventory.Get(outcomeData));
    }
}