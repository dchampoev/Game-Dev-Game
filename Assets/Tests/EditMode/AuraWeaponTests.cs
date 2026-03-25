using NUnit.Framework;
using UnityEngine;

public class AuraWeaponTests
{
    private class TestAuraWeapon : AuraWeapon
    {
        public void CallUpdate()
        {
            Update();
        }

        public void SetCurrentStats(Weapon.Stats stats)
        {
            currentStats = stats;
        }

        public void SetOwner(PlayerStats player)
        {
            owner = player;
        }

        public void SetCurrentAura(Aura aura)
        {
            currentAura = aura;
        }

        public Aura GetCurrentAura()
        {
            return currentAura;
        }
    }

    [Test]
    public void Update_ShouldDoNothing()
    {
        GameObject weaponObject = new GameObject("AuraWeapon");
        TestAuraWeapon weapon = weaponObject.AddComponent<TestAuraWeapon>();

        Assert.DoesNotThrow(() => weapon.CallUpdate());
    }
}