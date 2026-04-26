using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PickUpPlayModeTests
{
    private class TestPickup : Pickup
    {
        public void CallStart()
        {
            typeof(Pickup)
                .GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(this, null);
        }

        public void CallUpdate()
        {
            typeof(Pickup)
                .GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(this, null);
        }

        public void CallOnDestroy()
        {
            typeof(Pickup)
                .GetMethod("OnDestroy", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(this, null);
        }

        public PlayerStats GetTarget()
        {
            return (PlayerStats)typeof(Pickup)
                .GetField("target", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(this);
        }

        public float GetSpeed()
        {
            return (float)typeof(Pickup)
                .GetField("speed", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(this);
        }

        public Vector2 GetInitialPosition()
        {
            return (Vector2)typeof(Pickup)
                .GetField("initialPosition", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(this);
        }
    }

    private void SetPrivateField(object target, string fieldName, object value)
    {
        target.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(target, value);
    }

    private PlayerStats CreatePlayer()
    {
        GameObject playerObject = new GameObject("Player");
        playerObject.SetActive(false);

        PlayerStats playerStats = playerObject.AddComponent<PlayerStats>();
        playerStats.enabled = false;

        GameObject collectorObject = new GameObject("Collector");
        collectorObject.transform.SetParent(playerObject.transform);
        collectorObject.AddComponent<CircleCollider2D>();
        PlayerCollector collector = collectorObject.AddComponent<PlayerCollector>();
        collector.enabled = false;

        GameObject inventoryObject = new GameObject("Inventory");
        inventoryObject.transform.SetParent(playerObject.transform);
        PlayerInventory inventory = inventoryObject.AddComponent<PlayerInventory>();
        inventory.weaponSlots = new System.Collections.Generic.List<PlayerInventory.Slot>();
        inventory.passiveSlots = new System.Collections.Generic.List<PlayerInventory.Slot>();
        inventory.availableWeapons = new System.Collections.Generic.List<WeaponData>();
        inventory.availablePassives = new System.Collections.Generic.List<PassiveData>();

        CharacterData characterData = ScriptableObject.CreateInstance<CharacterData>();

        CharacterData.Stats stats = new CharacterData.Stats
        {
            maxHealth = 20f,
            recovery = 1f,
            armor = 0f,
            moveSpeed = 5f,
            might = 1f,
            area = 1f,
            speed = 1f,
            duration = 1f,
            amount = 0,
            cooldown = 1f,
            luck = 1f,
            growth = 1f,
            greed = 1f,
            curse = 0f,
            magnet = 1f,
            revival = 0
        };

        playerStats.baseStats = stats;
        playerStats.Stats = stats;
        playerStats.CurrentHealth = 10f;

        SetPrivateField(playerStats, "inventory", inventory);
        SetPrivateField(playerStats, "collector", collector);
        SetPrivateField(playerStats, "characterData", characterData);
        SetPrivateField(playerStats, "health", 10f);

        return playerStats;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.Destroy(go);
        }

        yield return null;

        foreach (var data in Resources.FindObjectsOfTypeAll<CharacterData>())
        {
            Object.DestroyImmediate(data, true);
        }
    }

    [UnityTest]
    public IEnumerator Collect_WhenNoTarget_ShouldAssignTargetAndSpeed()
    {
        GameObject pickupObject = new GameObject("Pickup");
        TestPickup pickup = pickupObject.AddComponent<TestPickup>();

        PlayerStats player = CreatePlayer();

        bool result = pickup.Collect(player, 7f, 2f);

        Assert.IsTrue(result);
        Assert.AreSame(player, pickup.GetTarget());
        Assert.AreEqual(7f, pickup.GetSpeed());
        Assert.AreEqual(2f, pickup.lifespan);

        yield return null;
    }

    [UnityTest]
    public IEnumerator Collect_WhenAlreadyCollected_ShouldReturnFalse()
    {
        GameObject pickupObject = new GameObject("Pickup");
        TestPickup pickup = pickupObject.AddComponent<TestPickup>();

        PlayerStats firstPlayer = CreatePlayer();
        PlayerStats secondPlayer = CreatePlayer();

        bool firstResult = pickup.Collect(firstPlayer, 5f);
        bool secondResult = pickup.Collect(secondPlayer, 8f);

        Assert.IsTrue(firstResult);
        Assert.IsFalse(secondResult);
        Assert.AreSame(firstPlayer, pickup.GetTarget());

        yield return null;
    }

    [UnityTest]
    public IEnumerator Update_WhenNoTarget_ShouldUseBobbingPosition()
    {
        GameObject pickupObject = new GameObject("Pickup");
        TestPickup pickup = pickupObject.AddComponent<TestPickup>();
        pickup.bobbingAnimation = new Pickup.BobbingAnimation
        {
            frequency = 2f,
            direction = new Vector2(0f, 0.3f)
        };

        pickupObject.transform.position = Vector3.zero;
        pickup.CallStart();

        Vector3 before = pickupObject.transform.position;

        yield return null;

        pickup.CallUpdate();

        Vector2 initialPosition = pickup.GetInitialPosition();

        Assert.AreEqual(initialPosition.x, pickupObject.transform.position.x, 0.001f);
        Assert.AreNotEqual(before.y, pickupObject.transform.position.y);
    }

    [UnityTest]
    public IEnumerator Update_WhenTargetExists_ShouldMoveTowardsTarget()
    {
        GameObject pickupObject = new GameObject("Pickup");
        TestPickup pickup = pickupObject.AddComponent<TestPickup>();
        pickupObject.transform.position = Vector3.zero;

        PlayerStats player = CreatePlayer();
        player.transform.position = new Vector3(5f, 0f, 0f);

        pickup.Collect(player, 10f, 2f);

        Vector3 before = pickupObject.transform.position;

        yield return null;

        pickup.CallUpdate();

        Assert.Greater(pickupObject.transform.position.x, before.x);

        yield return null;
    }

    [UnityTest]
    public IEnumerator OnDestroy_WhenPickupHasBonuses_ShouldApplyExperienceAndHealth()
    {
        GameObject pickupObject = new GameObject("Pickup");
        TestPickup pickup = pickupObject.AddComponent<TestPickup>();

        PlayerStats player = CreatePlayer();
        player.experience = 0;
        player.experienceCap = 100;
        player.CurrentHealth = 5f;

        pickup.experience = 10;
        pickup.health = 3;

        pickup.Collect(player, 5f, 1f);
        pickup.CallOnDestroy();

        Assert.AreEqual(10, player.experience);
        Assert.AreEqual(8f, player.CurrentHealth);

        yield return null;
    }

    [UnityTest]
    public IEnumerator OnDestroy_WhenNoTarget_ShouldDoNothing()
    {
        GameObject pickupObject = new GameObject("Pickup");
        TestPickup pickup = pickupObject.AddComponent<TestPickup>();
        pickup.experience = 10;
        pickup.health = 5;

        pickup.CallOnDestroy();

        Assert.Pass();

        yield return null;
    }
}