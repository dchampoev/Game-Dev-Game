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

    private CharacterScriptableObject CreateCharacterData(GameObject startingWeapon)
    {
        CharacterScriptableObject data = ScriptableObject.CreateInstance<CharacterScriptableObject>();
        SetPrivateField(data, "maxHealth", 10f);
        SetPrivateField(data, "recovery", 0f);
        SetPrivateField(data, "moveSpeed", 1f);
        SetPrivateField(data, "might", 1f);
        SetPrivateField(data, "projectileSpeed", 1f);
        SetPrivateField(data, "magnet", 1f);
        SetPrivateField(data, "startingWeapon", startingWeapon);
        return data;
    }

    private PlayerStats CreateTestPlayer()
    {
        GameObject selectorGameObject = new GameObject("CharacterSelector");
        CharacterSelector selector = selectorGameObject.AddComponent<CharacterSelector>();

        WeaponController dummyWeapon = CreateDummyWeaponPrefab();
        selector.selectedCharacter = CreateCharacterData(dummyWeapon.gameObject);

        GameObject playerGameObject = new GameObject("Player");
        playerGameObject.tag = "Player";

        PlayerMovement movement = playerGameObject.AddComponent<PlayerMovement>();
        movement.enabled = false;

        InventoryManager inventory = playerGameObject.AddComponent<InventoryManager>();
        for (int i = 0; i < 6; i++)
        {
            inventory.weaponSlots.Add(null);
            inventory.passiveItemSlots.Add(null);

            GameObject weaponUiGameObject = new GameObject($"WeaponUI_{i}");
            Image weaponImage = weaponUiGameObject.AddComponent<Image>();
            weaponImage.enabled = false;
            inventory.weaponUISlots.Add(weaponImage);

            GameObject passiveUiGameObject = new GameObject($"PassiveUI_{i}");
            Image passiveImage = passiveUiGameObject.AddComponent<Image>();
            passiveImage.enabled = false;
            inventory.passiveItemUISlots.Add(passiveImage);
        }

        PlayerStats player = playerGameObject.AddComponent<PlayerStats>();
        player.enabled = false;

        return player;
    }

    private EnemyStats CreateTestEnemy()
    {
        GameObject enemyGameObject = new GameObject("Enemy");
        enemyGameObject.tag = "Enemy";

        enemyGameObject.AddComponent<SpriteRenderer>();
        enemyGameObject.AddComponent<EnemyMovement>();

        EnemyStats enemy = enemyGameObject.AddComponent<EnemyStats>();
        enemy.currentHealth = 2f;
        enemy.currentDamage = 2f;
        enemy.currentMoveSpeed = 1f;

        return enemy;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Time.timeScale = 1f;

        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.Destroy(go);
        }

        yield return null;

        foreach (var data in Resources.FindObjectsOfTypeAll<ScriptableObject>())
        {
            if (data is CharacterScriptableObject || data is EnemyScriptableObject)
            {
                Object.DestroyImmediate(data);
            }
        }

        foreach (var data in Resources.FindObjectsOfTypeAll<WeaponScriptableObject>())
        {
            Object.DestroyImmediate(data);
        }
    }

    [UnityTest]
    public IEnumerator TakeDamage_WhenHealthDropsToZero_ShouldDestroyEnemy()
    {
        CreateTestPlayer();

        GameObject spawnerGO = new GameObject("Spawner");
        EnemySpawner spawner = spawnerGO.AddComponent<EnemySpawner>();
        spawner.relativeSpawnPoints = new List<Transform>();
        spawner.waves = new List<EnemySpawner.Wave>
        {
            new EnemySpawner.Wave { enemyGroups = new List<EnemySpawner.EnemyGroup>() }
        };

        EnemyStats enemy = CreateTestEnemy();

        yield return null;

        enemy.TakeDamage(2f, Vector2.zero, 0f, 0f);

        yield return new WaitForSecondsRealtime(0.8f);

        Assert.IsTrue(enemy == null || enemy.gameObject == null);
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
        spawner.waves = new List<EnemySpawner.Wave>
        {
            new EnemySpawner.Wave { enemyGroups = new List<EnemySpawner.EnemyGroup>() }
        };

        EnemyStats enemy = CreateTestEnemy();
        enemy.transform.position = new Vector3(100f, 100f, 0f);
        enemy.relocateDistance = 10f;

        yield return null;
        yield return null;

        Vector3 expected = player.transform.position + spawnPoint.transform.position;

        Assert.That(enemy.transform.position.x, Is.EqualTo(expected.x).Within(0.01f));
        Assert.That(enemy.transform.position.y, Is.EqualTo(expected.y).Within(0.01f));
        Assert.That(enemy.transform.position.z, Is.EqualTo(expected.z).Within(0.01f));
    }

    [UnityTest]
    public IEnumerator TakeDamage_WhenDamageIsLessThanHealth_ShouldOnlyReduceHealth()
    {
        CreateTestPlayer();

        GameObject enemyObject = new GameObject("Enemy");
        enemyObject.tag = "Enemy";
        enemyObject.AddComponent<SpriteRenderer>();
        enemyObject.AddComponent<EnemyMovement>();

        EnemyStats stats = enemyObject.AddComponent<EnemyStats>();
        stats.currentHealth = 10f;
        stats.currentDamage = 1f;
        stats.currentMoveSpeed = 1f;

        yield return null;

        stats.TakeDamage(3f, Vector2.zero, 0f, 0f);

        yield return null;

        Assert.AreEqual(7f, stats.currentHealth);
    }
}