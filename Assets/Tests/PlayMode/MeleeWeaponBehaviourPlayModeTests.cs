// using System.Collections;
// using System.Collections.Generic;
// using System.Reflection;
// using NUnit.Framework;
// using UnityEngine;
// using UnityEngine.TestTools;
// using UnityEngine.UI;

// public class MeleeWeaponBehaviourPlayModeTests
// {
//     private class TestMeleeWeaponBehaviour : MeleeWeaponBehaviour
//     {
//         public void CallOnTriggerEnter2D(Collider2D collider)
//         {
//             typeof(MeleeWeaponBehaviour)
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

//     private void SetPrivateFloat(MeleeWeaponBehaviour behaviour, string fieldName, float value)
//     {
//         typeof(MeleeWeaponBehaviour)
//             .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
//             ?.SetValue(behaviour, value);
//     }

//     private void SetPrivateInt(MeleeWeaponBehaviour behaviour, string fieldName, int value)
//     {
//         typeof(MeleeWeaponBehaviour)
//             .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
//             ?.SetValue(behaviour, value);
//     }

//     private WeaponScriptableObject CreateWeaponData(
//         float damage = 1f,
//         float speed = 1f,
//         float cooldown = 1f,
//         int pierce = 1)
//     {
//         WeaponScriptableObject data = ScriptableObject.CreateInstance<WeaponScriptableObject>();
//         SetPrivateField(data, "damage", damage);
//         SetPrivateField(data, "speed", speed);
//         SetPrivateField(data, "cooldownDuration", cooldown);
//         SetPrivateField(data, "pierce", pierce);
//         SetPrivateField(data, "level", 1);
//         return data;
//     }

//     private WeaponController CreateDummyWeaponPrefab()
//     {
//         GameObject prefab = new GameObject("DummyWeaponPrefab");
//         WeaponController controller = prefab.AddComponent<WeaponController>();

//         WeaponScriptableObject weaponData = CreateWeaponData();
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
//         GameObject selectorGameObject = new GameObject("CharacterSelector");
//         CharacterSelector selector = selectorGameObject.AddComponent<CharacterSelector>();

//         WeaponController dummyWeapon = CreateDummyWeaponPrefab();
//         selector.selectedCharacter = CreateCharacterData(dummyWeapon.gameObject);

//         GameObject playerGameObject = new GameObject("Player");
//         playerGameObject.tag = "Player";

//         PlayerMovement movement = playerGameObject.AddComponent<PlayerMovement>();
//         movement.enabled = false;

//         InventoryManager inventory = playerGameObject.AddComponent<InventoryManager>();
//         for (int i = 0; i < 6; i++)
//         {
//             inventory.weaponSlots.Add(null);
//             inventory.passiveItemSlots.Add(null);

//             GameObject weaponUiGameObject = new GameObject($"WeaponUI_{i}");
//             Image weaponImage = weaponUiGameObject.AddComponent<Image>();
//             weaponImage.enabled = false;
//             inventory.weaponUISlots.Add(weaponImage);

//             GameObject passiveUiGameObject = new GameObject($"PassiveUI_{i}");
//             Image passiveImage = passiveUiGameObject.AddComponent<Image>();
//             passiveImage.enabled = false;
//             inventory.passiveItemUISlots.Add(passiveImage);
//         }

//         PlayerStats player = playerGameObject.AddComponent<PlayerStats>();
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

//         foreach (var data in Resources.FindObjectsOfTypeAll<ScriptableObject>())
//         {
//             if (data is WeaponScriptableObject || data is CharacterScriptableObject)
//             {
//                 Object.DestroyImmediate(data);
//             }
//         }
//     }

//     [UnityTest]
//     public IEnumerator Start_ShouldDestroyObjectAfterLifetime()
//     {
//         GameObject weaponObject = new GameObject("MeleeWeapon");
//         MeleeWeaponBehaviour behaviour = weaponObject.AddComponent<MeleeWeaponBehaviour>();

//         behaviour.weaponData = CreateWeaponData(damage: 1f, speed: 1f, cooldown: 1f, pierce: 1);
//         behaviour.InitializeStats();
//         behaviour.lifetimeSeconds = 0.01f;

//         yield return new WaitForSecondsRealtime(0.05f);

//         Assert.IsTrue(behaviour == null || weaponObject == null);
//     }

//     [UnityTest]
//     public IEnumerator OnTriggerEnter2D_WhenWeaponHitsEnemy_ShouldReduceEnemyHealth()
//     {
//         CreateTestPlayer();

//         GameObject weaponObject = new GameObject("MeleeWeapon");
//         TestMeleeWeaponBehaviour behaviour = weaponObject.AddComponent<TestMeleeWeaponBehaviour>();

//         behaviour.lifetimeSeconds = 10f;
//         SetPrivateFloat(behaviour, "currentDamage", 2f);
//         SetPrivateInt(behaviour, "currentPierce", 2);

//         GameObject enemyObject = new GameObject("Enemy");
//         enemyObject.tag = "Enemy";
//         enemyObject.AddComponent<SpriteRenderer>();
//         enemyObject.AddComponent<EnemyMovement>();
//         BoxCollider2D collider = enemyObject.AddComponent<BoxCollider2D>();

//         EnemyStats enemyStats = enemyObject.AddComponent<EnemyStats>();
//         enemyStats.currentHealth = 10f;
//         enemyStats.currentDamage = 1f;
//         enemyStats.currentMoveSpeed = 1f;

//         yield return null;

//         behaviour.CallOnTriggerEnter2D(collider);

//         yield return null;

//         Assert.AreEqual(8f, enemyStats.currentHealth);
//     }
// }