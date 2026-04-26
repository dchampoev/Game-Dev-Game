using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class LightningRingWeaponTests
{
    private class TestLightningRing : LightningRingWeapon
    {
        public bool CallAttack(int attackCount = 1)
        {
            return (bool)typeof(LightningRingWeapon)
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

    private PlayerStats CreateOwner()
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
            speed = 1f,
            amount = 0
        };

        return owner;
    }

    [TearDown]
    public void TearDown()
    {
        LogAssert.ignoreFailingMessages = false;

        foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(obj);
        }

        foreach (var data in Resources.FindObjectsOfTypeAll<WeaponData>())
        {
            Object.DestroyImmediate(data, true);
        }
    }

    [Test]
    public void Attack_WhenHitEffectIsNull_ShouldReturnFalse()
    {
        GameObject obj = new GameObject("LightningRing");
        TestLightningRing weapon = obj.AddComponent<TestLightningRing>();
        weapon.enabled = false;

        weapon.SetOwner(CreateOwner());

        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
        data.baseStats = new Weapon.Stats
        {
            cooldown = 2f
        };
        weapon.data = data;

        weapon.SetCurrentStats(new Weapon.Stats
        {
            hitEffect = null,
            cooldown = 2f
        });

        LogAssert.Expect(LogType.Warning, "Lightning Ring weapon has no projectile prefab.");

        bool result = weapon.CallAttack(1);

        Assert.IsFalse(result);
        Assert.AreEqual(2f, weapon.GetCurrentCooldown());
    }

    [Test]
    public void Attack_WhenMultipleShots_ShouldQueueNextAttack()
    {
        GameObject obj = new GameObject("LightningRing");
        TestLightningRing weapon = obj.AddComponent<TestLightningRing>();
        weapon.enabled = false;

        weapon.SetOwner(CreateOwner());

        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
        data.baseStats = new Weapon.Stats
        {
            cooldown = 2f,
            projectileInterval = 0.5f
        };
        weapon.data = data;

        GameObject fxObject = new GameObject("Effect");
        ParticleSystem fx = fxObject.AddComponent<ParticleSystem>();

        weapon.SetCurrentStats(new Weapon.Stats
        {
            hitEffect = fx,
            cooldown = 2f,
            projectileInterval = 0.5f
        });

        weapon.SetCurrentCooldown(0f);

        bool result = weapon.CallAttack(3);

        Assert.IsTrue(result);
        Assert.AreEqual(2, weapon.GetCurrentAttackCount());
        Assert.AreEqual(0.5f, weapon.GetCurrentAttackInterval());
    }
}