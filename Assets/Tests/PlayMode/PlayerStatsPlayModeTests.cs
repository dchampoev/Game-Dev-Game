// using NUnit.Framework;
// using System.Collections;
// using System.Collections.Generic;
// using System.Reflection;
// using TMPro;
// using UnityEngine;
// using UnityEngine.TestTools;
// using UnityEngine.UI;

// public class PlayerStatsPlayModeTests
// {
//     private void SetPrivateField(object obj, string fieldName, object value)
//     {
//         obj.GetType()
//             .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
//             ?.SetValue(obj, value);
//     }

//     private T GetPrivateField<T>(object obj, string fieldName)
//     {
//         return (T)obj.GetType()
//             .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
//             ?.GetValue(obj);
//     }

//     private void CallPrivateMethod(object obj, string methodName)
//     {
//         obj.GetType()
//             .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
//             ?.Invoke(obj, null);
//     }

//     private Sprite CreateSprite()
//     {
//         Texture2D texture = new Texture2D(2, 2);
//         texture.SetPixel(0, 0, Color.white);
//         texture.SetPixel(1, 0, Color.white);
//         texture.SetPixel(0, 1, Color.white);
//         texture.SetPixel(1, 1, Color.white);
//         texture.Apply();

//         return Sprite.Create(texture, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
//     }

//     private TMP_Text CreateText(string name)
//     {
//         GameObject go = new GameObject(name);
//         return go.AddComponent<TextMeshProUGUI>();
//     }

//     private Image CreateImage(string name)
//     {
//         GameObject go = new GameObject(name);
//         Image image = go.AddComponent<Image>();
//         image.enabled = false;
//         return image;
//     }

//     private WeaponScriptableObject CreateWeaponData(
//         string weaponName = "Weapon",
//         string description = "Desc",
//         int level = 1,
//         Sprite icon = null,
//         GameObject nextLevelPrefab = null)
//     {
//         WeaponScriptableObject data = ScriptableObject.CreateInstance<WeaponScriptableObject>();

//         SetPrivateField(data, "name", weaponName);
//         SetPrivateField(data, "description", description);
//         SetPrivateField(data, "level", level);
//         SetPrivateField(data, "icon", icon);
//         SetPrivateField(data, "nextLevelPrefab", nextLevelPrefab);
//         SetPrivateField(data, "damage", 1f);
//         SetPrivateField(data, "speed", 1f);
//         SetPrivateField(data, "cooldownDuration", 1f);
//         SetPrivateField(data, "pierce", 1);
//         SetPrivateField(data, "evolvedUpgradeToRemove", 0);

//         return data;
//     }

//     private PassiveItemScriptableObject CreatePassiveItemData(
//         string itemName = "Passive",
//         string description = "Desc",
//         int level = 1,
//         Sprite icon = null,
//         GameObject nextLevelPrefab = null)
//     {
//         PassiveItemScriptableObject data = ScriptableObject.CreateInstance<PassiveItemScriptableObject>();

//         SetPrivateField(data, "name", itemName);
//         SetPrivateField(data, "description", description);
//         SetPrivateField(data, "level", level);
//         SetPrivateField(data, "icon", icon);
//         SetPrivateField(data, "nextLevelPrefab", nextLevelPrefab);
//         SetPrivateField(data, "multiplier", 1f);

//         return data;
//     }

//     private WeaponController CreateWeaponPrefab(string weaponName = "Knife", int level = 1, Sprite icon = null)
//     {
//         GameObject go = new GameObject(weaponName);
//         WeaponController controller = go.AddComponent<WeaponController>();
//         controller.weaponData = CreateWeaponData(weaponName, weaponName + " desc", level, icon);
//         return controller;
//     }

//     private PassiveItem CreatePassivePrefab(string itemName = "Armor", int level = 1, Sprite icon = null)
//     {
//         GameObject go = new GameObject(itemName);
//         PassiveItem passiveItem = go.AddComponent<PassiveItem>();
//         passiveItem.passiveItemData = CreatePassiveItemData(itemName, itemName + " desc", level, icon);
//         return passiveItem;
//     }

//     private CharacterScriptableObject CreateCharacterData(
//         float maxHealth,
//         float recovery,
//         float moveSpeed,
//         float might,
//         float projectileSpeed,
//         float magnet,
//         GameObject startingWeapon,
//         Sprite icon)
//     {
//         CharacterScriptableObject data = ScriptableObject.CreateInstance<CharacterScriptableObject>();

//         SetPrivateField(data, "maxHealth", maxHealth);
//         SetPrivateField(data, "recovery", recovery);
//         SetPrivateField(data, "moveSpeed", moveSpeed);
//         SetPrivateField(data, "might", might);
//         SetPrivateField(data, "projectileSpeed", projectileSpeed);
//         SetPrivateField(data, "magnet", magnet);
//         SetPrivateField(data, "startingWeapon", startingWeapon);
//         SetPrivateField(data, "icon", icon);

//         return data;
//     }

//     private GameManager CreateGameManager()
//     {
//         GameObject gameObject = new GameObject("GameManager");
//         gameObject.SetActive(false);

//         GameManager gameManager = gameObject.AddComponent<GameManager>();

//         gameManager.pauseMenu = new GameObject("PauseMenu");
//         gameManager.resultsScreen = new GameObject("ResultsScreen");
//         gameManager.levelUpScreen = new GameObject("LevelUpScreen");

//         gameManager.currentHealthDisplay = (TextMeshProUGUI)CreateText("CurrentHealthDisplay");
//         gameManager.currentRecoveryDisplay = (TextMeshProUGUI)CreateText("CurrentRecoveryDisplay");
//         gameManager.currentMoveSpeedDisplay = (TextMeshProUGUI)CreateText("CurrentMoveSpeedDisplay");
//         gameManager.currentMightDisplay = (TextMeshProUGUI)CreateText("CurrentMightDisplay");
//         gameManager.currentProjectileSpeedDisplay = (TextMeshProUGUI)CreateText("CurrentProjectileSpeedDisplay");
//         gameManager.currentMagnetDisplay = (TextMeshProUGUI)CreateText("CurrentMagnetDisplay");

//         gameManager.chosenCharacterImage = CreateImage("ChosenCharacterImage");
//         gameManager.chosenCharacterName = (TextMeshProUGUI)CreateText("ChosenCharacterName");
//         gameManager.levelReachedDisplay = (TextMeshProUGUI)CreateText("LevelReachedDisplay");
//         gameManager.timeSurvivedDisplay = (TextMeshProUGUI)CreateText("TimeSurvivedDisplay");
//         gameManager.stopwatchDisplay = (TextMeshProUGUI)CreateText("StopwatchDisplay");

//         for (int i = 0; i < 6; i++)
//         {
//             gameManager.chosenWeaponsUI.Add(CreateImage($"ChosenWeaponUI_{i}"));
//             gameManager.chosenPassiveItemsUI.Add(CreateImage($"ChosenPassiveUI_{i}"));
//         }

//         gameObject.SetActive(true);
//         return gameManager;
//     }

//     private InventoryManager CreateInventory(GameObject playerObject)
//     {
//         InventoryManager inventory = playerObject.AddComponent<InventoryManager>();

//         for (int i = 0; i < 6; i++)
//         {
//             inventory.weaponSlots.Add(null);
//             inventory.passiveItemSlots.Add(null);
//             inventory.weaponUISlots.Add(CreateImage($"WeaponUI_{i}"));
//             inventory.passiveItemUISlots.Add(CreateImage($"PassiveUI_{i}"));
//         }

//         return inventory;
//     }

//     private PlayerStats CreatePlayerEnvironment(
//         out GameManager gameManager,
//         out InventoryManager inventory,
//         out CharacterScriptableObject characterData)
//     {
//         gameManager = CreateGameManager();

//         GameObject selectorGO = new GameObject("CharacterSelector");
//         CharacterSelector selector = selectorGO.AddComponent<CharacterSelector>();

//         Sprite characterIcon = CreateSprite();
//         Sprite weaponIcon = CreateSprite();

//         WeaponController startingWeapon = CreateWeaponPrefab("StarterKnife", 1, weaponIcon);

//         characterData = CreateCharacterData(
//             maxHealth: 10f,
//             recovery: 2f,
//             moveSpeed: 5f,
//             might: 3f,
//             projectileSpeed: 4f,
//             magnet: 6f,
//             startingWeapon: startingWeapon.gameObject,
//             icon: characterIcon);

//         selector.selectedCharacter = characterData;

//         GameObject playerGO = new GameObject("Player");
//         inventory = CreateInventory(playerGO);

//         PlayerStats player = playerGO.AddComponent<PlayerStats>();
//         player.healthBar = CreateImage("HealthBar");
//         player.expBar = CreateImage("ExpBar");
//         player.levelText = (TextMeshProUGUI)CreateText("LevelText");
//         player.levelRanges = new List<PlayerStats.LevelRange>
//         {
//             new PlayerStats.LevelRange { startLevel = 1, endLevel = 2, experienceCapIncrease = 5 },
//             new PlayerStats.LevelRange { startLevel = 3, endLevel = 10, experienceCapIncrease = 7 }
//         };

//         gameManager.playerObject = playerGO;

//         return player;
//     }

//     [UnityTearDown]
//     public IEnumerator TearDown()
//     {
//         Time.timeScale = 1f;

//         foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
//         {
//             Object.Destroy(go);
//         }

//         yield return null;

//         foreach (var data in Resources.FindObjectsOfTypeAll<ScriptableObject>())
//         {
//             if (data is WeaponScriptableObject ||
//                 data is PassiveItemScriptableObject ||
//                 data is CharacterScriptableObject)
//             {
//                 Object.DestroyImmediate(data);
//             }
//         }
//     }

//     [UnityTest]
//     public IEnumerator AwakeAndStart_ShouldInitializeStatsUiBarsAndStartingWeapon()
//     {
//         PlayerStats player = CreatePlayerEnvironment(out GameManager gm, out InventoryManager inventory, out CharacterScriptableObject _);

//         yield return null;

//         Assert.AreEqual(10f, player.CurrentHealth);
//         Assert.AreEqual(2f, player.CurrentRecovery);
//         Assert.AreEqual(5f, player.CurrentMoveSpeed);
//         Assert.AreEqual(3f, player.CurrentMight);
//         Assert.AreEqual(4f, player.CurrentProjectileSpeed);
//         Assert.AreEqual(6f, player.CurrentMagnet);

//         Assert.AreEqual(5, player.experienceCap);
//         Assert.AreEqual("LV 1", player.levelText.text);
//         Assert.AreEqual(1f, player.healthBar.fillAmount);
//         Assert.AreEqual(0f, player.expBar.fillAmount);

//         Assert.AreEqual("Health: 10", gm.currentHealthDisplay.text);
//         Assert.AreEqual("Recovery: 2.0", gm.currentRecoveryDisplay.text);
//         Assert.AreEqual("Move Speed: 5.0", gm.currentMoveSpeedDisplay.text);
//         Assert.AreEqual("Might: 3.0", gm.currentMightDisplay.text);
//         Assert.AreEqual("Projectile Speed: 4.0", gm.currentProjectileSpeedDisplay.text);
//         Assert.AreEqual("Magnet: 6.0", gm.currentMagnetDisplay.text);

//         Assert.IsNotNull(inventory.weaponSlots[0]);
//         Assert.AreEqual(1, player.weaponIndex);
//     }

//     [UnityTest]
//     public IEnumerator IncreaseExperience_WhenBelowCap_ShouldIncreaseExperienceAndUpdateBar()
//     {
//         PlayerStats player = CreatePlayerEnvironment(out _, out _, out _);

//         yield return null;

//         player.IncreaseExperience(2);

//         Assert.AreEqual(2, player.experience);
//         Assert.AreEqual(1, player.level);
//         Assert.AreEqual(0.4f, player.expBar.fillAmount, 0.001f);
//     }

//     [UnityTest]
//     public IEnumerator IncreaseExperience_WhenReachingCap_ShouldLevelUpIncreaseCapAndPauseForUpgrade()
//     {
//         PlayerStats player = CreatePlayerEnvironment(out GameManager gm, out _, out _);

//         yield return null;

//         player.IncreaseExperience(5);

//         Assert.AreEqual(0, player.experience);
//         Assert.AreEqual(2, player.level);
//         Assert.AreEqual(10, player.experienceCap);
//         Assert.AreEqual("LV 2", player.levelText.text);
//         Assert.AreEqual(GameManager.GameState.LevelUp, gm.currentState);
//     }

//     [UnityTest]
//     public IEnumerator TakeDamage_WhenNotInvincible_ShouldReduceHealthUpdateBarAndSetIFrames()
//     {
//         Time.timeScale = 1f;

//         PlayerStats player = CreatePlayerEnvironment(out _, out _, out _);
//         player.iFrameDuration = 1f;

//         yield return null;

//         player.TakeDamage(3f);

//         Assert.AreEqual(7f, player.CurrentHealth);
//         Assert.AreEqual(0.7f, player.healthBar.fillAmount, 0.001f);
//         Assert.IsTrue(GetPrivateField<bool>(player, "isInvincible"));
//         Assert.AreEqual(1f, GetPrivateField<float>(player, "iFrameTimer"));
//     }

//     [UnityTest]
//     public IEnumerator TakeDamage_WhenInvincible_ShouldNotReduceHealthAgain()
//     {
//         PlayerStats player = CreatePlayerEnvironment(out _, out _, out _);
//         player.iFrameDuration = 1f;

//         yield return null;

//         player.TakeDamage(3f);
//         player.TakeDamage(3f);

//         Assert.AreEqual(7f, player.CurrentHealth);
//     }

//     [UnityTest]
//     public IEnumerator Heal_WhenBelowMax_ShouldIncreaseHealthButClampToMax()
//     {
//         PlayerStats player = CreatePlayerEnvironment(out _, out _, out _);

//         yield return null;

//         player.CurrentHealth = 8f;
//         player.Heal(5f);

//         Assert.AreEqual(10f, player.CurrentHealth);
//     }

//     [UnityTest]
//     public IEnumerator Recover_WhenBelowMax_ShouldIncreaseHealth()
//     {
//         PlayerStats player = CreatePlayerEnvironment(out _, out _, out _);

//         yield return null;

//         player.CurrentHealth = 5f;
//         CallPrivateMethod(player, "Recover");

//         Assert.Greater(player.CurrentHealth, 5f);
//         Assert.LessOrEqual(player.CurrentHealth, 10f);
//     }

//     [UnityTest]
//     public IEnumerator Die_WhenCalled_ShouldAssignResultsAndSetGameOverState()
//     {
//         PlayerStats player = CreatePlayerEnvironment(out GameManager gm, out InventoryManager inventory, out _);

//         yield return null;

//         inventory.weaponUISlots[0].enabled = true;
//         inventory.weaponUISlots[0].sprite = CreateSprite();
//         inventory.passiveItemUISlots[0].enabled = true;
//         inventory.passiveItemUISlots[0].sprite = CreateSprite();
//         gm.stopwatchDisplay.text = "03:21";

//         player.level = 4;
//         player.Die();

//         Assert.AreEqual("4", gm.levelReachedDisplay.text);
//         Assert.AreEqual("03:21", gm.timeSurvivedDisplay.text);
//         Assert.AreEqual(GameManager.GameState.GameOver, gm.currentState);
//     }

//     [UnityTest]
//     public IEnumerator SpawnWeapon_WhenSlotAvailable_ShouldInstantiateWeaponAndIncreaseIndex()
//     {
//         Time.timeScale = 1f;

//         PlayerStats player = CreatePlayerEnvironment(out _, out InventoryManager inventory, out _);

//         yield return null;

//         Sprite icon = CreateSprite();
//         WeaponController weaponPrefab = CreateWeaponPrefab("ExtraWeapon", 2, icon);

//         player.SpawnWeapon(weaponPrefab.gameObject);

//         Assert.AreEqual(2, player.weaponIndex);
//         Assert.IsNotNull(inventory.weaponSlots[1]);
//         Assert.AreEqual(2, inventory.weaponSlots[1].weaponData.Level);
//     }

//     [UnityTest]
//     public IEnumerator SpawnWeapon_WhenNoSlotsAvailable_ShouldLogError()
//     {
//         PlayerStats player = CreatePlayerEnvironment(out _, out _, out _);

//         yield return null;

//         player.weaponIndex = 6;
//         WeaponController weaponPrefab = CreateWeaponPrefab("OverflowWeapon", 1, CreateSprite());

//         LogAssert.Expect(LogType.Error, "No more weapon slots available!");
//         player.SpawnWeapon(weaponPrefab.gameObject);
//     }

//     [UnityTest]
//     public IEnumerator SpawnPassiveItem_WhenSlotAvailable_ShouldInstantiatePassiveAndIncreaseIndex()
//     {
//         PlayerStats player = CreatePlayerEnvironment(out _, out InventoryManager inventory, out _);

//         yield return null;

//         PassiveItem passivePrefab = CreatePassivePrefab("Armor", 2, CreateSprite());

//         player.SpawnPassiveItem(passivePrefab.gameObject);

//         Assert.AreEqual(1, player.passiveItemIndex);
//         Assert.IsNotNull(inventory.passiveItemSlots[0]);
//         Assert.AreEqual(2, inventory.passiveItemSlots[0].passiveItemData.Level);
//     }

//     [UnityTest]
//     public IEnumerator SpawnPassiveItem_WhenNoSlotsAvailable_ShouldLogError()
//     {
//         PlayerStats player = CreatePlayerEnvironment(out _, out _, out _);

//         yield return null;

//         player.passiveItemIndex = 6;
//         PassiveItem passivePrefab = CreatePassivePrefab("OverflowPassive", 1, CreateSprite());

//         LogAssert.Expect(LogType.Error, "No more passive item slots available!");
//         player.SpawnPassiveItem(passivePrefab.gameObject);
//     }

//     [UnityTest]
//     public IEnumerator UpdateExpBar_WhenExperienceChanges_ShouldReflectRatio()
//     {
//         PlayerStats player = CreatePlayerEnvironment(out _, out _, out _);

//         yield return null;

//         player.experience = 3;
//         player.experienceCap = 6;
//         CallPrivateMethod(player, "UpdateExpBar");

//         Assert.AreEqual(0.5f, player.expBar.fillAmount, 0.001f);
//     }

//     [UnityTest]
//     public IEnumerator UpdateLevelText_WhenLevelChanges_ShouldRefreshText()
//     {
//         PlayerStats player = CreatePlayerEnvironment(out _, out _, out _);

//         yield return null;

//         player.level = 7;
//         CallPrivateMethod(player, "UpdateLevelText");

//         Assert.AreEqual("LV 7", player.levelText.text);
//     }
// }