using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class WhipWeaponTests
{
    private class TestWhipWeapon : WhipWeapon
    {
        public bool CallAttack(int attackCount = 1)
        {
            return (bool)typeof(WhipWeapon)
                .GetMethod("Attack", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(this, new object[] { attackCount });
        }

        public void SetCurrentStats(Weapon.Stats value)
        {
            typeof(Weapon)
                .GetField("currentStats", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(this, value);
        }

        public float GetCurrentCooldown()
        {
            return (float)typeof(Weapon)
                .GetField("currentCooldown", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(this);
        }
    }

    [Test]
    public void Attack_WhenProjectilePrefabIsMissing_ShouldReturnFalseAndSetCooldown()
    {
        GameObject weaponObject = new GameObject("Whip");
        TestWhipWeapon weapon = weaponObject.AddComponent<TestWhipWeapon>();

        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
        data.baseStats = new Weapon.Stats
        {
            cooldown = 2f
        };
        data.linearGrowth = new Weapon.Stats[0];
        data.randomGrowth = new Weapon.Stats[0];
        weapon.data = data;

        weapon.SetCurrentStats(new Weapon.Stats
        {
            projectilePrefab = null
        });

        LogAssert.Expect(LogType.Warning, "Whip weapon has no projectile prefab.");

        bool result = weapon.CallAttack(1);

        Assert.IsFalse(result);
        Assert.AreEqual(2f, weapon.GetCurrentCooldown());
    }
}