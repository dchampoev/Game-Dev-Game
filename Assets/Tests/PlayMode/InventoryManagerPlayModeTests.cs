using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class InventoryManagerPlayModeTests
{
    private void SetPrivateField(object obj, string fieldName, object value)
    {
        obj.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(obj, value);
    }

    private Sprite CreateSprite()
    {
        Texture2D texture = new Texture2D(2, 2);
        texture.SetPixel(0, 0, Color.white);
        texture.SetPixel(1, 0, Color.white);
        texture.SetPixel(0, 1, Color.white);
        texture.SetPixel(1, 1, Color.white);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
    }

    private WeaponScriptableObject CreateWeaponData(
        string weaponName = "Weapon",
        string description = "Desc",
        int level = 1,
        Sprite icon = null,
        GameObject nextLevelPrefab = null,
        int evolvedUpgradeToRemove = 0)
    {
        WeaponScriptableObject data = ScriptableObject.CreateInstance<WeaponScriptableObject>();

        SetPrivateField(data, "name", weaponName);
        SetPrivateField(data, "description", description);
        SetPrivateField(data, "level", level);
        SetPrivateField(data, "icon", icon);
        SetPrivateField(data, "nextLevelPrefab", nextLevelPrefab);
        SetPrivateField(data, "evolvedUpgradeToRemove", evolvedUpgradeToRemove);
        SetPrivateField(data, "damage", 1f);
        SetPrivateField(data, "speed", 1f);
        SetPrivateField(data, "cooldownDuration", 1f);
        SetPrivateField(data, "pierce", 1);

        return data;
    }

    private PassiveItemScriptableObject CreatePassiveItemData(
        string itemName = "Passive",
        string description = "Desc",
        int level = 1,
        Sprite icon = null,
        GameObject nextLevelPrefab = null)
    {
        PassiveItemScriptableObject data = ScriptableObject.CreateInstance<PassiveItemScriptableObject>();

        SetPrivateField(data, "name", itemName);
        SetPrivateField(data, "description", description);
        SetPrivateField(data, "level", level);
        SetPrivateField(data, "icon", icon);
        SetPrivateField(data, "nextLevelPrefab", nextLevelPrefab);
        SetPrivateField(data, "multiplier", 1f);

        return data;
    }

    private WeaponController CreateWeaponController(
        string weaponName,
        int level,
        Sprite icon,
        GameObject nextLevelPrefab = null,
        int evolvedUpgradeToRemove = 0)
    {
        GameObject go = new GameObject(weaponName);
        WeaponController controller = go.AddComponent<WeaponController>();
        controller.weaponData = CreateWeaponData(
            weaponName,
            weaponName + " desc",
            level,
            icon,
            nextLevelPrefab,
            evolvedUpgradeToRemove);

        return controller;
    }

    private PassiveItem CreatePassiveItem(
        string itemName,
        int level,
        Sprite icon,
        GameObject nextLevelPrefab = null)
    {
        GameObject go = new GameObject(itemName);
        PassiveItem passiveItem = go.AddComponent<PassiveItem>();
        passiveItem.passiveItemData = CreatePassiveItemData(
            itemName,
            itemName + " desc",
            level,
            icon,
            nextLevelPrefab);

        return passiveItem;
    }

    private InventoryManager CreateInventoryManager()
    {
        GameObject player = new GameObject("Player");
        InventoryManager inventory = player.AddComponent<InventoryManager>();

        for (int i = 0; i < 6; i++)
        {
            inventory.weaponSlots.Add(null);
            inventory.passiveItemSlots.Add(null);

            GameObject weaponUiGo = new GameObject($"WeaponUI_{i}");
            Image weaponUi = weaponUiGo.AddComponent<Image>();
            weaponUi.enabled = false;
            inventory.weaponUISlots.Add(weaponUi);

            GameObject passiveUiGo = new GameObject($"PassiveUI_{i}");
            Image passiveUi = passiveUiGo.AddComponent<Image>();
            passiveUi.enabled = false;
            inventory.passiveItemUISlots.Add(passiveUi);
        }

        return inventory;
    }

    private WeaponEvolutionBlueprint CreateEvolutionBlueprint(
        WeaponScriptableObject weaponData,
        PassiveItemScriptableObject passiveData,
        GameObject evolvedWeapon,
        WeaponScriptableObject evolvedWeaponData = null)
    {
        WeaponEvolutionBlueprint blueprint = ScriptableObject.CreateInstance<WeaponEvolutionBlueprint>();
        blueprint.weaponToEvolveData = weaponData;
        blueprint.catalystPassiveItemData = passiveData;
        blueprint.evolvedWeapon = evolvedWeapon;
        blueprint.evolvedWeaponData = evolvedWeaponData;
        return blueprint;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.Destroy(go);
        }

        yield return null;

        foreach (var weaponData in Resources.FindObjectsOfTypeAll<WeaponScriptableObject>())
        {
            Object.DestroyImmediate(weaponData);
        }

        foreach (var passiveData in Resources.FindObjectsOfTypeAll<PassiveItemScriptableObject>())
        {
            Object.DestroyImmediate(passiveData);
        }

        foreach (var blueprint in Resources.FindObjectsOfTypeAll<WeaponEvolutionBlueprint>())
        {
            Object.DestroyImmediate(blueprint);
        }
    }

    [UnityTest]
    public IEnumerator LevelUpWeapon_WhenWeaponHasNextLevel_ShouldReplaceWeaponAndUpdateChosenUpgrade()
    {
        InventoryManager inventory = CreateInventoryManager();

        Sprite baseIcon = CreateSprite();
        Sprite upgradedIcon = CreateSprite();

        WeaponController upgradedWeapon = CreateWeaponController("Knife Lv2", 2, upgradedIcon);
        GameObject upgradedPrefab = upgradedWeapon.gameObject;

        WeaponController currentWeapon = CreateWeaponController("Knife Lv1", 1, baseIcon, upgradedPrefab);
        inventory.weaponSlots[0] = currentWeapon;
        inventory.weaponLevels[0] = 1;

        InventoryManager.WeaponUpgrade chosenUpgrade = new InventoryManager.WeaponUpgrade
        {
            weaponUpgradeIndex = 0,
            initialWeapon = currentWeapon.gameObject,
            weaponData = currentWeapon.weaponData
        };

        inventory.LevelUpWeapon(0, chosenUpgrade);

        yield return null;

        Assert.IsNotNull(inventory.weaponSlots[0]);
        Assert.AreEqual(2, inventory.weaponLevels[0]);
        Assert.AreEqual(2, inventory.weaponSlots[0].weaponData.Level);
        Assert.AreEqual(inventory.weaponSlots[0].weaponData, chosenUpgrade.weaponData);
        Assert.AreEqual(upgradedIcon, inventory.weaponUISlots[0].sprite);
    }

    [UnityTest]
    public IEnumerator LevelUpPassiveItem_WhenPassiveItemHasNextLevel_ShouldReplacePassiveItemAndUpdateChosenUpgrade()
    {
        InventoryManager inventory = CreateInventoryManager();

        Sprite baseIcon = CreateSprite();
        Sprite upgradedIcon = CreateSprite();

        PassiveItem upgradedItem = CreatePassiveItem("Armor Lv2", 2, upgradedIcon);
        GameObject upgradedPrefab = upgradedItem.gameObject;

        PassiveItem currentItem = CreatePassiveItem("Armor Lv1", 1, baseIcon, upgradedPrefab);
        inventory.passiveItemSlots[0] = currentItem;
        inventory.passiveItemsLevels[0] = 1;

        InventoryManager.PassiveItemUpgrade chosenUpgrade = new InventoryManager.PassiveItemUpgrade
        {
            passiveItemUpgradeIndex = 0,
            initialPassiveItem = currentItem.gameObject,
            passiveItemData = currentItem.passiveItemData
        };

        inventory.LevelUpPassiveItem(0, chosenUpgrade);

        yield return null;

        Assert.IsNotNull(inventory.passiveItemSlots[0]);
        Assert.AreEqual(2, inventory.passiveItemsLevels[0]);
        Assert.AreEqual(2, inventory.passiveItemSlots[0].passiveItemData.Level);
        Assert.AreEqual(inventory.passiveItemSlots[0].passiveItemData, chosenUpgrade.passiveItemData);
        Assert.AreEqual(upgradedIcon, inventory.passiveItemUISlots[0].sprite);
    }

    [UnityTest]
    public IEnumerator GetPossibleEvolutions_WhenRequirementsAreMet_ShouldReturnEvolution()
    {
        InventoryManager inventory = CreateInventoryManager();

        Sprite weaponIcon = CreateSprite();
        Sprite passiveIcon = CreateSprite();
        Sprite evolvedIcon = CreateSprite();

        WeaponController weapon = CreateWeaponController("Whip", 3, weaponIcon);
        PassiveItem catalyst = CreatePassiveItem("Heart", 2, passiveIcon);
        WeaponController evolvedWeapon = CreateWeaponController("Bloody Tear", 5, evolvedIcon);

        inventory.weaponSlots[0] = weapon;
        inventory.passiveItemSlots[0] = catalyst;

        WeaponEvolutionBlueprint blueprint = CreateEvolutionBlueprint(
            weapon.weaponData,
            catalyst.passiveItemData,
            evolvedWeapon.gameObject,
            evolvedWeapon.weaponData);

        inventory.weaponEvolutions.Add(blueprint);

        yield return null;

        List<WeaponEvolutionBlueprint> result = inventory.GetPossibleEvolutions();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(blueprint, result[0]);
    }

    [UnityTest]
    public IEnumerator GetPossibleEvolutions_WhenRequirementsAreNotMet_ShouldReturnEmptyList()
    {
        InventoryManager inventory = CreateInventoryManager();

        Sprite weaponIcon = CreateSprite();
        Sprite passiveIcon = CreateSprite();
        Sprite evolvedIcon = CreateSprite();

        WeaponController weapon = CreateWeaponController("Whip", 1, weaponIcon);
        PassiveItem catalyst = CreatePassiveItem("Heart", 1, passiveIcon);
        WeaponController evolvedWeapon = CreateWeaponController("Bloody Tear", 5, evolvedIcon);

        inventory.weaponSlots[0] = weapon;
        inventory.passiveItemSlots[0] = catalyst;

        WeaponScriptableObject requiredWeaponData = CreateWeaponData("Whip Required", "req", 3, weaponIcon);
        PassiveItemScriptableObject requiredPassiveData = CreatePassiveItemData("Heart Required", "req", 2, passiveIcon);

        WeaponEvolutionBlueprint blueprint = CreateEvolutionBlueprint(
            requiredWeaponData,
            requiredPassiveData,
            evolvedWeapon.gameObject,
            evolvedWeapon.weaponData);

        inventory.weaponEvolutions.Add(blueprint);

        yield return null;

        List<WeaponEvolutionBlueprint> result = inventory.GetPossibleEvolutions();

        Assert.AreEqual(0, result.Count);
    }

    [UnityTest]
    public IEnumerator EvolveWeapon_WhenMatchingEvolutionExists_ShouldReplaceWeaponAndRemoveUpgradeOption()
    {
        InventoryManager inventory = CreateInventoryManager();

        Sprite weaponIcon = CreateSprite();
        Sprite passiveIcon = CreateSprite();
        Sprite evolvedIcon = CreateSprite();

        WeaponController weapon = CreateWeaponController("Whip", 3, weaponIcon);
        PassiveItem catalyst = CreatePassiveItem("Heart", 2, passiveIcon);

        inventory.weaponSlots[0] = weapon;
        inventory.passiveItemSlots[0] = catalyst;
        inventory.weaponLevels[0] = weapon.weaponData.Level;

        inventory.weaponUISlots[0].enabled = true;
        inventory.weaponUISlots[0].sprite = weaponIcon;

        InventoryManager.WeaponUpgrade removableUpgrade = new InventoryManager.WeaponUpgrade
        {
            weaponUpgradeIndex = 0,
            initialWeapon = weapon.gameObject,
            weaponData = weapon.weaponData
        };
        inventory.weaponUpgradeOptions.Add(removableUpgrade);

        WeaponController evolvedWeaponController = CreateWeaponController(
            "Bloody Tear",
            5,
            evolvedIcon,
            null,
            0);

        WeaponEvolutionBlueprint blueprint = CreateEvolutionBlueprint(
            weapon.weaponData,
            catalyst.passiveItemData,
            evolvedWeaponController.gameObject,
            evolvedWeaponController.weaponData);

        inventory.EvolveWeapon(blueprint);

        yield return null;

        Assert.IsNotNull(inventory.weaponSlots[0]);
        Assert.AreEqual(5, inventory.weaponLevels[0]);
        Assert.AreEqual(evolvedIcon, inventory.weaponUISlots[0].sprite);
        Assert.AreEqual(0, inventory.weaponUpgradeOptions.Count);
    }

    [UnityTest]
    public IEnumerator EvolveWeapon_WhenRequirementsDoNotMatch_ShouldKeepOriginalWeapon()
    {
        InventoryManager inventory = CreateInventoryManager();

        Sprite weaponIcon = CreateSprite();
        Sprite passiveIcon = CreateSprite();
        Sprite evolvedIcon = CreateSprite();

        WeaponController weapon = CreateWeaponController("Whip", 1, weaponIcon);
        PassiveItem catalyst = CreatePassiveItem("Heart", 1, passiveIcon);

        inventory.weaponSlots[0] = weapon;
        inventory.passiveItemSlots[0] = catalyst;
        inventory.weaponLevels[0] = weapon.weaponData.Level;
        inventory.weaponUISlots[0].sprite = weaponIcon;

        InventoryManager.WeaponUpgrade upgrade = new InventoryManager.WeaponUpgrade
        {
            weaponUpgradeIndex = 0,
            initialWeapon = weapon.gameObject,
            weaponData = weapon.weaponData
        };
        inventory.weaponUpgradeOptions.Add(upgrade);

        WeaponScriptableObject requiredWeaponData = CreateWeaponData("Whip Required", "req", 3, weaponIcon);
        PassiveItemScriptableObject requiredPassiveData = CreatePassiveItemData("Heart Required", "req", 2, passiveIcon);

        WeaponController evolvedWeaponController = CreateWeaponController("Bloody Tear", 5, evolvedIcon, null, 0);

        WeaponEvolutionBlueprint blueprint = CreateEvolutionBlueprint(
            requiredWeaponData,
            requiredPassiveData,
            evolvedWeaponController.gameObject,
            evolvedWeaponController.weaponData);

        inventory.EvolveWeapon(blueprint);

        yield return null;

        Assert.AreEqual(weapon, inventory.weaponSlots[0]);
        Assert.AreEqual(1, inventory.weaponLevels[0]);
        Assert.AreEqual(1, inventory.weaponUpgradeOptions.Count);
        Assert.AreEqual(weaponIcon, inventory.weaponUISlots[0].sprite);
    }
}