using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class ItemAttemptEvolutionPlayModeTests
{
    private readonly List<ScriptableObject> createdScriptableObjects = new List<ScriptableObject>();

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
        createdScriptableObjects.Add(data);
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
        createdScriptableObjects.Add(data);
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

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.Destroy(go);
        }

        yield return null;

        foreach (var obj in createdScriptableObjects)
        {
            if (obj != null)
            {
                Object.DestroyImmediate(obj, true);
            }
        }

        foreach (var obj in Resources.FindObjectsOfTypeAll<CharacterData>())
        {
            Object.DestroyImmediate(obj, true);
        }

        createdScriptableObjects.Clear();
    }

    [UnityTest]
    public IEnumerator AttemptEvolution_WhenValid_ShouldRemoveCatalystAndAddOutcome()
    {
        GameObject inventoryObject = new GameObject("Inventory");
        PlayerInventory inventory = inventoryObject.AddComponent<PlayerInventory>();
        inventory.weaponSlots = new List<PlayerInventory.Slot>();
        inventory.passiveSlots = new List<PlayerInventory.Slot>();
        inventory.availableWeapons = new List<WeaponData>();
        inventory.availablePassives = new List<PassiveData>();
        inventory.upgradeUIOptions = new List<PlayerInventory.UpgradeUI>();

        PlayerStats stats = PlayerTestFactory.CreatePlayerStats(inventory);
        PlayerTestFactory.SetPrivateField(inventory, "player", stats);

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