using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class PlayerInventoryRemovePassivePlayModeTests
{
    private class TestPassive : Passive
    {
        public bool unequipped;

        public override void OnUnequip()
        {
            unequipped = true;
        }
    }

    private PlayerInventory CreateInventory()
    {
        GameObject go = new GameObject("Inventory");
        PlayerInventory inventory = go.AddComponent<PlayerInventory>();
        inventory.weaponSlots = new List<PlayerInventory.Slot>();
        inventory.passiveSlots = new List<PlayerInventory.Slot>();
        inventory.availableWeapons = new List<WeaponData>();
        inventory.availablePassives = new List<PassiveData>();
        return inventory;
    }

    private PassiveData CreatePassiveData(string name)
    {
        PassiveData data = ScriptableObject.CreateInstance<PassiveData>();
        data.name = name;
        data.maxLevel = 5;
        data.baseStats = new Passive.Modifier
        {
            name = name,
            description = name + "_Description"
        };
        data.growth = new Passive.Modifier[0];
        return data;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.Destroy(go);
        }

        yield return null;

        foreach (var data in Resources.FindObjectsOfTypeAll<PassiveData>())
        {
            Object.DestroyImmediate(data, true);
        }
    }

    [UnityTest]
    public IEnumerator RemovePassive_WhenPassiveExists_ShouldClearSlot_CallOnUnequip_AndDestroyObject()
    {
        PlayerInventory inventory = CreateInventory();
        PassiveData passiveData = CreatePassiveData("Spinach");

        GameObject passiveObject = new GameObject("Passive");
        TestPassive passive = passiveObject.AddComponent<TestPassive>();
        passive.data = passiveData;

        inventory.passiveSlots.Add(new PlayerInventory.Slot
        {
            item = passive,
        });

        bool result = inventory.Remove(passiveData);

        yield return null;

        Assert.IsTrue(result);
        Assert.IsTrue(passive.unequipped);
        Assert.IsTrue(inventory.passiveSlots[0].IsEmpty());
        Assert.IsTrue(passiveObject == null);
    }

    [UnityTest]
    public IEnumerator RemovePassive_WhenRemoveUpgradeAvailabilityIsTrue_ShouldAlsoRemoveFromAvailablePassives()
    {
        PlayerInventory inventory = CreateInventory();
        PassiveData passiveData = CreatePassiveData("Armor");
        inventory.availablePassives.Add(passiveData);

        GameObject passiveObject = new GameObject("Passive");
        TestPassive passive = passiveObject.AddComponent<TestPassive>();
        passive.data = passiveData;

        inventory.passiveSlots.Add(new PlayerInventory.Slot
        {
            item = passive,
        });

        bool result = inventory.Remove(passiveData, true);

        yield return null;

        Assert.IsTrue(result);
        Assert.IsFalse(inventory.availablePassives.Contains(passiveData));
        Assert.IsTrue(inventory.passiveSlots[0].IsEmpty());
    }

    [UnityTest]
    public IEnumerator RemovePassive_WhenPassiveDoesNotExist_ShouldReturnFalse()
    {
        PlayerInventory inventory = CreateInventory();
        PassiveData passiveData = CreatePassiveData("Clover");

        bool result = inventory.Remove(passiveData);

        yield return null;

        Assert.IsFalse(result);
    }
}