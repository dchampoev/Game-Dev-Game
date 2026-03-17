using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class EnemyStatsPlayModeTests
{
    private void SetPrivateField(object obj, string fieldName, object value)
    {
        obj.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(obj, value);
    }

    private T CreateScriptableWithFields<T>(Dictionary<string, object> fields) where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();

        foreach (var kvp in fields)
        {
            SetPrivateField(asset, kvp.Key, kvp.Value);
        }

        return asset;
    }

    private WeaponController CreateDummyWeaponPrefab()
    {
        GameObject prefab = new GameObject("DummyWeaponPrefab");
        WeaponController weaponController = prefab.AddComponent<WeaponController>();

        WeaponScriptableObject weaponData = ScriptableObject.CreateInstance<WeaponScriptableObject>();
        SetPrivateField(weaponData, "level", 1);
        SetPrivateField(weaponData, "damage", 1f);
        SetPrivateField(weaponData, "speed", 1f);
        SetPrivateField(weaponData, "cooldownDuration", 1f);
        SetPrivateField(weaponData, "pierce", 1);

        weaponController.weaponData = weaponData;
        return weaponController;
    }

    private PlayerStats CreateTestPlayer()
    {
        GameObject selectorGO = new GameObject("CharacterSelector");
        CharacterSelector selector = selectorGO.AddComponent<CharacterSelector>();

        WeaponController dummyWeapon = CreateDummyWeaponPrefab();

        CharacterScriptableObject characterData =
            CreateScriptableWithFields<CharacterScriptableObject>(new Dictionary<string, object>
            {
            { "maxHealth", 10f },
            { "recovery", 0f },
            { "moveSpeed", 1f },
            { "might", 1f },
            { "projectileSpeed", 1f },
            { "magnet", 1f },
            { "startingWeapon", dummyWeapon.gameObject }
            });

        selector.selectedCharacter = characterData;

        GameObject playerGO = new GameObject("Player");
        playerGO.tag = "Player";

        InventoryManager inventory = playerGO.AddComponent<InventoryManager>();

        for (int i = 0; i < 6; i++)
        {
            inventory.weaponSlots.Add(null);
            inventory.passiveItemSlots.Add(null);

            GameObject weaponUIImageGO = new GameObject($"WeaponUI_{i}");
            Image weaponImage = weaponUIImageGO.AddComponent<Image>();
            weaponImage.enabled = false;
            inventory.weaponUISlots.Add(weaponImage);

            GameObject passiveUIImageGO = new GameObject($"PassiveUI_{i}");
            Image passiveImage = passiveUIImageGO.AddComponent<Image>();
            passiveImage.enabled = false;
            inventory.passiveItemUISlots.Add(passiveImage);
        }

        PlayerStats player = playerGO.AddComponent<PlayerStats>();

        player.enabled = false;

        return player;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.Destroy(go);
        }

        yield return null;

        foreach (var data in Resources.FindObjectsOfTypeAll<EnemyScriptableObject>())
        {
            Object.DestroyImmediate(data);
        }

        foreach (var data in Resources.FindObjectsOfTypeAll<WeaponScriptableObject>())
        {
            Object.DestroyImmediate(data);
        }
    }

    [UnityTest]
    public IEnumerator TakeDamage_WhenHealthDropsToZero_ShouldDestroyEnemy()
    {
        PlayerStats player = CreateTestPlayer();

        GameObject spawnerGO = new GameObject("Spawner");
        EnemySpawner spawner = spawnerGO.AddComponent<EnemySpawner>();

        spawner.relativeSpawnPoints = new List<Transform>();
        spawner.waves = new List<EnemySpawner.Wave>()
    {
        new EnemySpawner.Wave()
        {
            enemyGroups = new List<EnemySpawner.EnemyGroup>()
        }
    };

        GameObject enemyGO = new GameObject("Enemy");
        EnemyStats enemy = enemyGO.AddComponent<EnemyStats>();
        enemy.currentHealth = 2f;

        yield return null;

        enemy.TakeDamage(2f);

        yield return null;

        Assert.IsTrue(enemy == null || enemyGO == null);
    }

    [UnityTest]
    public IEnumerator Update_WhenEnemyIsFar_ShouldRelocate()
    {
        PlayerStats player = CreateTestPlayer();

        GameObject spawnPoint = new GameObject("SpawnPoint");
        spawnPoint.transform.position = new Vector3(5f, 0f, 0f);

        GameObject spawnerGO = new GameObject("Spawner");
        EnemySpawner spawner = spawnerGO.AddComponent<EnemySpawner>();
        spawner.relativeSpawnPoints = new List<Transform> { spawnPoint.transform };
        spawner.waves = new List<EnemySpawner.Wave>()
    {
        new EnemySpawner.Wave()
        {
            enemyGroups = new List<EnemySpawner.EnemyGroup>()
        }
    };

        GameObject enemyGO = new GameObject("Enemy");
        EnemyStats enemy = enemyGO.AddComponent<EnemyStats>();

        enemy.transform.position = new Vector3(100f, 100f, 0f);
        enemy.relocateDistance = 10f;

        yield return null;

        yield return null;

        Assert.AreEqual(player.transform.position + spawnPoint.transform.position, enemy.transform.position);
    }

    [UnityTest]
    public IEnumerator OnCollisionStay2D_WhenCollidingWithPlayer_ShouldDamagePlayer()
    {
        PlayerStats player = CreateTestPlayer();

        player.CurrentHealth = 10f;

        Rigidbody2D playerRb = player.gameObject.AddComponent<Rigidbody2D>();
        playerRb.gravityScale = 0f;
        playerRb.freezeRotation = true;
        player.gameObject.AddComponent<BoxCollider2D>();

        GameObject enemyObject = new GameObject("Enemy");
        EnemyStats enemyStats = enemyObject.AddComponent<EnemyStats>();
        enemyStats.currentDamage = 2f;

        Rigidbody2D enemyRb = enemyObject.AddComponent<Rigidbody2D>();
        enemyRb.gravityScale = 0f;
        enemyRb.freezeRotation = true;
        enemyObject.AddComponent<BoxCollider2D>();

        player.transform.position = Vector3.zero;
        enemyObject.transform.position = Vector3.zero;

        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        Assert.AreEqual(8f, player.CurrentHealth);
    }
}