using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class UnholyVespersWeaponTests
{
    private class TestUnholyVespersWeapon : UnholyVespersWeapon
    {
        public void SetOwner(PlayerStats value)
        {
            typeof(Item)
                .GetField("owner", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(this, value);
        }

        public void SetCurrentStats(Weapon.Stats value)
        {
            typeof(Weapon)
                .GetField("currentStats", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(this, value);
        }

        public void SetCurrentCooldown(float value)
        {
            typeof(Weapon)
                .GetField("currentCooldown", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(this, value);
        }

        public float GetCurrentCooldown()
        {
            return (float)typeof(Weapon)
                .GetField("currentCooldown", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(this);
        }
    }

    private PlayerStats CreateOwner(float cooldown)
    {
        GameObject playerObject = new GameObject("Player");
        playerObject.SetActive(false);

        PlayerStats owner = playerObject.AddComponent<PlayerStats>();
        owner.enabled = false;
        owner.Stats = new CharacterData.Stats
        {
            cooldown = cooldown
        };

        return owner;
    }

    private TestUnholyVespersWeapon CreateWeapon(float currentCooldown = 0f)
    {
        GameObject weaponObject = new GameObject("UnholyVespers");
        TestUnholyVespersWeapon weapon = weaponObject.AddComponent<TestUnholyVespersWeapon>();
        weapon.SetOwner(CreateOwner(0.5f));
        weapon.SetCurrentCooldown(currentCooldown);
        weapon.SetCurrentStats(new Weapon.Stats
        {
            lifespan = 6f,
            cooldown = 99f
        });
        return weapon;
    }

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject obj in Object.FindObjectsByType<GameObject>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        ))
        {
            Object.DestroyImmediate(obj);
        }
    }

    [Test]
    public void ActivateCooldown_ShouldUseOnlyLifespanScaledByOwnerCooldown()
    {
        TestUnholyVespersWeapon weapon = CreateWeapon();

        bool result = weapon.ActivateCooldown(true);

        Assert.IsTrue(result);
        Assert.AreEqual(3f, weapon.GetCurrentCooldown());
    }

    [Test]
    public void ActivateCooldown_WhenStrictAndAlreadyCoolingDown_ShouldNotChangeCooldown()
    {
        TestUnholyVespersWeapon weapon = CreateWeapon(1.25f);

        bool result = weapon.ActivateCooldown(true);

        Assert.IsFalse(result);
        Assert.AreEqual(1.25f, weapon.GetCurrentCooldown());
    }
}
