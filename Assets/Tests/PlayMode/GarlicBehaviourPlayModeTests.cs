// using System.Collections;
// using System.Collections.Generic;
// using System.Reflection;
// using NUnit.Framework;
// using UnityEngine;
// using UnityEngine.TestTools;
// using UnityEngine.UI;

// public class GarlicBehaviourPlayModeTests
// {
//     private class TestGarlicBehaviour : GarlicBehaviour
//     {
//         public void InitializeMarkedEnemies()
//         {
//             typeof(GarlicBehaviour)
//                 .GetField("markedEnemies", BindingFlags.Instance | BindingFlags.NonPublic)
//                 ?.SetValue(this, new List<GameObject>());
//         }

//         public List<GameObject> GetMarkedEnemies()
//         {
//             return (List<GameObject>)typeof(GarlicBehaviour)
//                 .GetField("markedEnemies", BindingFlags.Instance | BindingFlags.NonPublic)
//                 ?.GetValue(this);
//         }

//         public void CallOnTriggerEnter2D(Collider2D collider)
//         {
//             typeof(GarlicBehaviour)
//                 .GetMethod("OnTriggerEnter2D", BindingFlags.Instance | BindingFlags.NonPublic)
//                 ?.Invoke(this, new object[] { collider });
//         }
//     }

//     private void SetPrivateField(object obj, string fieldName, object value)
//     {
//         obj.GetType()
//             .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
//             ?.SetValue(obj, value);
//     }

//     private WeaponController CreateDummyWeaponPrefab()
//     {
//         GameObject prefab = new GameObject("DummyWeaponPrefab");
//         WeaponController controller = prefab.AddComponent<WeaponController>();

//         WeaponScriptableObject weaponData = ScriptableObject.CreateInstance<WeaponScriptableObject>();
//         SetPrivateField(weaponData, "damage", 1f);
//         SetPrivateField(weaponData, "speed", 1f);
//         SetPrivateField(weaponData, "cooldownDuration", 1f);
//         SetPrivateField(weaponData, "pierce", 1);
//         SetPrivateField(weaponData, "level", 1);

//         controller.weaponData = weaponData;
//         return controller;
//     }

//     private CharacterScriptableObject CreateCharacterData(GameObject startingWeapon)
//     {
//         CharacterScriptableObject data = ScriptableObject.CreateInstance<CharacterScriptableObject>();
//         SetPrivateField(data, "maxHealth", 10f);
//         SetPrivateField(data, "recovery", 0f);
//         SetPrivateField(data, "moveSpeed", 1f);
//         SetPrivateField(data, "might", 1f);
//         SetPrivateField(data, "projectileSpeed", 1f);
//         SetPrivateField(data, "magnet", 1f);
//         SetPrivateField(data, "startingWeapon", startingWeapon);
//         return data;
//     }

//     private void CreateTestPlayer()
//     {
//         GameObject selectorGO = new GameObject("CharacterSelector");
//         CharacterSelector selector = selectorGO.AddComponent<CharacterSelector>();

//         WeaponController dummyWeapon = CreateDummyWeaponPrefab();
//         selector.selectedCharacter = CreateCharacterData(dummyWeapon.gameObject);

//         GameObject playerGO = new GameObject("Player");
//         playerGO.tag = "Player";

//         PlayerMovement movement = playerGO.AddComponent<PlayerMovement>();
//         movement.enabled = false;

//         InventoryManager inventory = playerGO.AddComponent<InventoryManager>();
//         for (int i = 0; i < 6; i++)
//         {
//             inventory.weaponSlots.Add(null);
//             inventory.passiveItemSlots.Add(null);

//             GameObject weaponUiGo = new GameObject($"WeaponUI_{i}");
//             Image weaponImage = weaponUiGo.AddComponent<Image>();
//             weaponImage.enabled = false;
//             inventory.weaponUISlots.Add(weaponImage);

//             GameObject passiveUiGo = new GameObject($"PassiveUI_{i}");
//             Image passiveImage = passiveUiGo.AddComponent<Image>();
//             passiveImage.enabled = false;
//             inventory.passiveItemUISlots.Add(passiveImage);
//         }

//         PlayerStats player = playerGO.AddComponent<PlayerStats>();
//         player.enabled = false;
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
//     }

//     [UnityTest]
//     public IEnumerator OnTriggerEnter2D_WhenEnemy_ShouldAddEnemyToMarkedEnemies()
//     {
//         CreateTestPlayer();

//         GameObject garlicObject = new GameObject("Garlic");
//         TestGarlicBehaviour garlic = garlicObject.AddComponent<TestGarlicBehaviour>();
//         garlic.InitializeMarkedEnemies();
//         garlic.lifetimeSeconds = 10f;

//         GameObject enemyObject = new GameObject("Enemy");
//         enemyObject.tag = "Enemy";
//         enemyObject.AddComponent<SpriteRenderer>();
//         enemyObject.AddComponent<EnemyMovement>();
//         BoxCollider2D enemyCollider = enemyObject.AddComponent<BoxCollider2D>();

//         EnemyStats enemyStats = enemyObject.AddComponent<EnemyStats>();
//         enemyStats.currentHealth = 10f;
//         enemyStats.currentDamage = 1f;
//         enemyStats.currentMoveSpeed = 1f;

//         yield return null;

//         garlic.CallOnTriggerEnter2D(enemyCollider);

//         yield return null;

//         Assert.AreEqual(1, garlic.GetMarkedEnemies().Count);
//         Assert.AreEqual(enemyObject, garlic.GetMarkedEnemies()[0]);
//     }
// }