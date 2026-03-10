using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;

public class PlayerAndInventoryTests
{
    private GameObject playerObject;
    private PlayerStats playerStats;
    private InventoryManager inventoryManager;

    [SetUp]
    public void Setup()
    {
        playerObject = new GameObject("Player");

        inventoryManager = playerObject.AddComponent<InventoryManager>();
        playerStats = playerObject.AddComponent<PlayerStats>();

        inventoryManager.weaponSlots = new List<WeaponController> { null, null, null, null, null, null };
        inventoryManager.passiveItemSlots = new List<PassiveItem> { null, null, null, null, null, null };

        inventoryManager.weaponUISlots = new List<Image>();
        inventoryManager.passiveItemUISlots = new List<Image>();

        for (int i = 0; i < 6; i++)
        {
            var weaponImageGO = new GameObject($"WeaponImage{i}");
            weaponImageGO.AddComponent<Canvas>();
            inventoryManager.weaponUISlots.Add(weaponImageGO.AddComponent<Image>());

            var passiveImageGO = new GameObject($"PassiveImage{i}");
            passiveImageGO.AddComponent<Canvas>();
            inventoryManager.passiveItemUISlots.Add(passiveImageGO.AddComponent<Image>());
        }

        playerStats.levelRanges = new List<PlayerStats.LevelRange>
        {
            new PlayerStats.LevelRange { startLevel = 1, endLevel = 10, experienceCapIncrease = 10 }
        };

        playerStats.experience = 0;
        playerStats.level = 1;
        playerStats.experienceCap = 10;
        playerStats.CurrentHealth = 100f;
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(playerObject);
    }

    [Test]
    public void IncreaseExperience_WhenBelowCap_ShouldNotLevelUp()
    {
        playerStats.IncreaseExperience(5);

        Assert.AreEqual(5, playerStats.experience);
        Assert.AreEqual(1, playerStats.level);
    }

    [Test]
    public void IncreaseExperience_WhenReachingCap_ShouldLevelUp()
    {
        playerStats.IncreaseExperience(10);

        Assert.AreEqual(2, playerStats.level);
        Assert.AreEqual(0, playerStats.experience);
        Assert.AreEqual(20, playerStats.experienceCap);
    }

    [Test]
    public void TakeDamage_ShouldReduceHealth()
    {
        float initialHealth = playerStats.CurrentHealth;

        playerStats.TakeDamage(20f);

        Assert.AreEqual(initialHealth - 20f, playerStats.CurrentHealth);
    }

    [Test]
    public void AddWeapon_ShouldPlaceWeaponInCorrectSlot()
    {
        GameObject weaponGO = new GameObject("Weapon");
        WeaponController weaponController = weaponGO.AddComponent<WeaponController>();

        WeaponScriptableObject weaponData =
            Resources.Load<WeaponScriptableObject>("Weapon/Knife Weapon");

        weaponController.weaponData = weaponData;

        inventoryManager.AddWeapon(0, weaponController);

        Assert.AreEqual(weaponController, inventoryManager.weaponSlots[0]);
        Assert.AreEqual(weaponData.Level, inventoryManager.weaponLevels[0]);

        Object.DestroyImmediate(weaponGO);
    }

    [Test]
    public void AddPassiveItem_ShouldPlacePassiveItemInCorrectSlot()
    {
        GameObject inventoryGO = new GameObject("Inventory");
        InventoryManager inventoryManager = inventoryGO.AddComponent<InventoryManager>();

        inventoryManager.passiveItemSlots = new System.Collections.Generic.List<PassiveItem>() { null, null, null, null, null, null };
        inventoryManager.passiveItemUISlots = new System.Collections.Generic.List<UnityEngine.UI.Image>()
        {
            new GameObject().AddComponent<UnityEngine.UI.Image>(),
            new GameObject().AddComponent<UnityEngine.UI.Image>(),
            new GameObject().AddComponent<UnityEngine.UI.Image>(),
            new GameObject().AddComponent<UnityEngine.UI.Image>(),
            new GameObject().AddComponent<UnityEngine.UI.Image>(),
            new GameObject().AddComponent<UnityEngine.UI.Image>()
        };

        GameObject passiveGO = new GameObject("PassiveItem");
        PassiveItem passiveItem = passiveGO.AddComponent<PassiveItem>();

        PassiveItemScriptableObject passiveData =
            Resources.Load<PassiveItemScriptableObject>("Passive Item/Wings Passive Item");

        passiveItem.passiveItemData = passiveData;

        inventoryManager.AddPassiveItem(0, passiveItem);

        Assert.AreEqual(passiveItem, inventoryManager.passiveItemSlots[0]);
        Assert.AreEqual(passiveData.Level, inventoryManager.passiveItemsLevels[0]);

        Object.DestroyImmediate(passiveGO);
        Object.DestroyImmediate(inventoryGO);
    }
}