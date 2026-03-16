using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class WeaponEvolutionTests
{
    private GameObject inventoryObject;
    private InventoryManager inventoryManager;

    private WeaponEvolutionBlueprint evolutionBlueprint;
    private WeaponScriptableObject requiredWeaponData;
    private PassiveItemScriptableObject requiredPassiveData;
    private WeaponScriptableObject evolvedWeaponData;

    [SetUp]
    public void Setup()
    {
        inventoryObject = new GameObject("Inventory");
        inventoryManager = inventoryObject.AddComponent<InventoryManager>();

        inventoryManager.weaponSlots = new List<WeaponController> { null, null, null, null, null, null };
        inventoryManager.passiveItemSlots = new List<PassiveItem> { null, null, null, null, null, null };
        inventoryManager.weaponUISlots = new List<Image>();
        inventoryManager.passiveItemUISlots = new List<Image>();
        inventoryManager.weaponEvolutions = new List<WeaponEvolutionBlueprint>();

        for (int i = 0; i < 6; i++)
        {
            inventoryManager.weaponUISlots.Add(new GameObject($"WeaponUI{i}").AddComponent<Image>());
            inventoryManager.passiveItemUISlots.Add(new GameObject($"PassiveUI{i}").AddComponent<Image>());
        }

        requiredWeaponData = CreateWeaponDataWithLevel(3);
        requiredPassiveData = CreatePassiveDataWithLevel(1);
        evolvedWeaponData = CreateWeaponDataWithLevel(1);

        evolutionBlueprint = ScriptableObject.CreateInstance<WeaponEvolutionBlueprint>();
        evolutionBlueprint.weaponToEvolveData = requiredWeaponData;
        evolutionBlueprint.catalystPassiveItemData = requiredPassiveData;
        evolutionBlueprint.evolvedWeaponData = evolvedWeaponData;

        inventoryManager.weaponEvolutions.Add(evolutionBlueprint);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(requiredWeaponData);
        Object.DestroyImmediate(requiredPassiveData);
        Object.DestroyImmediate(evolvedWeaponData);
        Object.DestroyImmediate(evolutionBlueprint);
        Object.DestroyImmediate(inventoryObject);
    }

    [Test]
    public void GetPossibleEvolutions_WhenWeaponIsLevel3AndPassiveExists_ShouldReturnEvolution()
    {
        GameObject weaponObject = new GameObject("Weapon");
        WeaponController weaponController = weaponObject.AddComponent<WeaponController>();
        weaponController.weaponData = CreateWeaponDataWithLevel(3);

        GameObject passiveObject = new GameObject("Passive");
        PassiveItem passiveItem = passiveObject.AddComponent<PassiveItem>();
        passiveItem.passiveItemData = CreatePassiveDataWithLevel(1);

        inventoryManager.weaponSlots[0] = weaponController;
        inventoryManager.passiveItemSlots[0] = passiveItem;

        List<WeaponEvolutionBlueprint> result = inventoryManager.GetPossibleEvolutions();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(evolutionBlueprint, result[0]);

        Object.DestroyImmediate(weaponController.weaponData);
        Object.DestroyImmediate(passiveItem.passiveItemData);
        Object.DestroyImmediate(weaponObject);
        Object.DestroyImmediate(passiveObject);
    }

    [Test]
    public void GetPossibleEvolutions_WhenWeaponLevelIsTooLow_ShouldReturnEmptyList()
    {
        GameObject weaponObject = new GameObject("Weapon");
        WeaponController weaponController = weaponObject.AddComponent<WeaponController>();
        weaponController.weaponData = CreateWeaponDataWithLevel(2);

        GameObject passiveObject = new GameObject("Passive");
        PassiveItem passiveItem = passiveObject.AddComponent<PassiveItem>();
        passiveItem.passiveItemData = CreatePassiveDataWithLevel(1);

        inventoryManager.weaponSlots[0] = weaponController;
        inventoryManager.passiveItemSlots[0] = passiveItem;

        List<WeaponEvolutionBlueprint> result = inventoryManager.GetPossibleEvolutions();

        Assert.AreEqual(0, result.Count);

        Object.DestroyImmediate(weaponController.weaponData);
        Object.DestroyImmediate(passiveItem.passiveItemData);
        Object.DestroyImmediate(weaponObject);
        Object.DestroyImmediate(passiveObject);
    }

    [Test]
    public void GetPossibleEvolutions_WhenPassiveItemIsMissing_ShouldReturnEmptyList()
    {
        GameObject weaponObject = new GameObject("Weapon");
        WeaponController weaponController = weaponObject.AddComponent<WeaponController>();
        weaponController.weaponData = CreateWeaponDataWithLevel(3);

        inventoryManager.weaponSlots[0] = weaponController;
        inventoryManager.passiveItemSlots[0] = null;

        List<WeaponEvolutionBlueprint> result = inventoryManager.GetPossibleEvolutions();

        Assert.AreEqual(0, result.Count);

        Object.DestroyImmediate(weaponController.weaponData);
        Object.DestroyImmediate(weaponObject);
    }

    private WeaponScriptableObject CreateWeaponDataWithLevel(int level)
    {
        WeaponScriptableObject weaponData = ScriptableObject.CreateInstance<WeaponScriptableObject>();
        SerializedObject serializedObject = new SerializedObject(weaponData);
        serializedObject.FindProperty("level").intValue = level;
        serializedObject.ApplyModifiedProperties();
        return weaponData;
    }

    private PassiveItemScriptableObject CreatePassiveDataWithLevel(int level)
    {
        PassiveItemScriptableObject passiveData = ScriptableObject.CreateInstance<PassiveItemScriptableObject>();
        SerializedObject serializedObject = new SerializedObject(passiveData);
        serializedObject.FindProperty("level").intValue = level;
        serializedObject.ApplyModifiedProperties();
        return passiveData;
    }
}