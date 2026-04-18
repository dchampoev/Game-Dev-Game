using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerTestFactory
{
    public static void SetPrivateField(object target, string fieldName, object value)
    {
        target.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(target, value);
    }

    public static PlayerStats CreatePlayerStats(PlayerInventory inventory = null)
    {
        GameObject playerObject = new GameObject("Player");
        playerObject.SetActive(false);

        PlayerStats playerStats = playerObject.AddComponent<PlayerStats>();
        playerStats.enabled = false;

        if (inventory == null)
        {
            inventory = playerObject.AddComponent<PlayerInventory>();
            inventory.weaponSlots = new List<PlayerInventory.Slot>();
            inventory.passiveSlots = new List<PlayerInventory.Slot>();
            inventory.availableWeapons = new List<WeaponData>();
            inventory.availablePassives = new List<PassiveData>();
            inventory.upgradeUIOptions = new List<PlayerInventory.UpgradeUI>();
        }
        else
        {
            if (inventory.gameObject != playerObject)
            {
                inventory.transform.SetParent(playerObject.transform);
            }
        }

        GameObject collectorObject = new GameObject("Collector");
        collectorObject.transform.SetParent(playerObject.transform);
        collectorObject.AddComponent<CircleCollider2D>();
        PlayerCollector collector = collectorObject.AddComponent<PlayerCollector>();
        collector.enabled = false;

        CharacterData characterData = ScriptableObject.CreateInstance<CharacterData>();

        CharacterData.Stats stats = new CharacterData.Stats
        {
            maxHealth = 20f,
            recovery = 1f,
            moveSpeed = 5f,
            might = 1f,
            speed = 1f,
            magnet = 1f
        };

        playerStats.baseStats = stats;

        SetPrivateField(playerStats, "inventory", inventory);
        SetPrivateField(playerStats, "collector", collector);
        SetPrivateField(playerStats, "characterData", characterData);
        SetPrivateField(playerStats, "actualStats", stats);
        SetPrivateField(playerStats, "health", 20f);

        return playerStats;
    }
}