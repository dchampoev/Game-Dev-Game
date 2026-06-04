using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ThunderLoopWeaponTests
{
    private class TestThunderLoopWeapon : ThunderLoopWeapon
    {
        public bool CallAttack(int attackCount = 1)
        {
            return (bool)typeof(ThunderLoopWeapon)
                .GetMethod("Attack", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(this, new object[] { attackCount });
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

        public int GetCurrentAttackCount()
        {
            return (int)typeof(ProjectileWeapon)
                .GetField("currentAttackCount", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(this);
        }

        public float GetCurrentAttackInterval()
        {
            return (float)typeof(ProjectileWeapon)
                .GetField("currentAttackInterval", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(this);
        }

        public void SetOwner(PlayerStats value)
        {
            typeof(Item)
                .GetField("owner", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(this, value);
        }
    }

    [TearDown]
    public void TearDown()
    {
        LogAssert.ignoreFailingMessages = false;

        foreach (GameObject obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(obj);
        }

        TestScriptableObjectCleanup.DestroyRuntimeObjects<WeaponData>();
    }

    [Test]
    public void Attack_WhenHitEffectIsMissing_ShouldReturnFalseAndRefreshCooldown()
    {
        TestThunderLoopWeapon weapon = CreateWeapon();

        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
        data.baseStats = new Weapon.Stats
        {
            cooldown = 2f
        };
        weapon.data = data;

        weapon.SetCurrentStats(new Weapon.Stats
        {
            cooldown = 2f
        });

        LogAssert.Expect(LogType.Warning, "Thunder Loop weapon has no hit effect assigned.");

        bool result = weapon.CallAttack();

        Assert.IsFalse(result);
        Assert.AreEqual(2f, weapon.GetCurrentCooldown());
    }

    [Test]
    public void Attack_WhenMultipleShotsAndNoTargets_ShouldQueueNextAttack()
    {
        TestThunderLoopWeapon weapon = CreateWeapon();

        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
        data.baseStats = new Weapon.Stats
        {
            cooldown = 2f,
            projectileInterval = 0.25f
        };
        weapon.data = data;

        ParticleSystem hitEffect = new GameObject("ThunderLoopHitEffect").AddComponent<ParticleSystem>();
        weapon.SetCurrentStats(new Weapon.Stats
        {
            hitEffect = hitEffect,
            cooldown = 2f,
            projectileInterval = 0.25f
        });

        weapon.SetCurrentCooldown(0f);

        bool result = weapon.CallAttack(3);

        Assert.IsTrue(result);
        Assert.AreEqual(2, weapon.GetCurrentAttackCount());
        Assert.AreEqual(0.25f, weapon.GetCurrentAttackInterval());
    }

    private static TestThunderLoopWeapon CreateWeapon()
    {
        GameObject weaponObject = new GameObject("ThunderLoop");
        TestThunderLoopWeapon weapon = weaponObject.AddComponent<TestThunderLoopWeapon>();
        weapon.enabled = false;
        weapon.SetOwner(CreateOwner());
        return weapon;
    }

    private static PlayerStats CreateOwner()
    {
        GameObject playerObject = new GameObject("Player");
        playerObject.SetActive(false);

        PlayerStats owner = playerObject.AddComponent<PlayerStats>();
        owner.enabled = false;
        owner.Stats = new CharacterData.Stats
        {
            cooldown = 1f,
            might = 1f,
            area = 1f,
            duration = 1f,
            speed = 1f,
            amount = 0
        };

        return owner;
    }
}
