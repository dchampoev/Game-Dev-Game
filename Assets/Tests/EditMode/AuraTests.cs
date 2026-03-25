using NUnit.Framework;
using UnityEngine;

public class AuraTests
{
    private class TestWeapon : Weapon
    {
        public Stats stats;
        public float damage = 5f;

        public override Stats GetStats()
        {
            return stats;
        }

        public override float GetDamage()
        {
            return damage;
        }
    }

    [Test]
    public void GetDamage_ShouldReturnWeaponDamage()
    {
        GameObject auraObject = new GameObject("Aura");
        Aura aura = auraObject.AddComponent<Aura>();

        GameObject weaponObject = new GameObject("Weapon");
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();
        weapon.damage = 7f;

        aura.weapon = weapon;

        float result = aura.GetDamage();

        Assert.AreEqual(7f, result);

        Object.DestroyImmediate(auraObject);
        Object.DestroyImmediate(weaponObject);
    }
}