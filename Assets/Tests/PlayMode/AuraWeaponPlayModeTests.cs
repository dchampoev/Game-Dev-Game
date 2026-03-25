using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class AuraWeaponPlayModeTests
{
    private class TestAuraWeapon : AuraWeapon
    {
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

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.Destroy(go);
        }

        yield return null;
    }

    [UnityTest]
    public IEnumerator OnEquip_WhenAuraPrefabExists_ShouldInstantiateAuraAndAssignFields()
    {
        GameObject playerObject = new GameObject("Player");
        PlayerStats owner = playerObject.AddComponent<PlayerStats>();
        owner.enabled = false;

        GameObject auraPrefabObject = new GameObject("AuraPrefab");
        Aura auraPrefab = auraPrefabObject.AddComponent<Aura>();

        GameObject weaponObject = new GameObject("AuraWeapon");
        TestAuraWeapon weapon = weaponObject.AddComponent<TestAuraWeapon>();
        weapon.SetOwner(owner);
        weapon.SetCurrentStats(new Weapon.Stats
        {
            auraPrefab = auraPrefab,
            area = 3f
        });

        weapon.OnEquip();

        yield return null;

        Aura currentAura = weapon.GetCurrentAura();

        Assert.NotNull(currentAura);
        Assert.AreEqual(weapon, currentAura.weapon);
        Assert.AreEqual(owner, currentAura.owner);
        Assert.AreEqual(weapon.transform, currentAura.transform.parent);
        Assert.AreEqual(new Vector3(3f, 3f, 3f), currentAura.transform.localScale);
    }

    [UnityTest]
    public IEnumerator OnUnequip_WhenCurrentAuraExists_ShouldDestroyAura()
    {
        GameObject weaponObject = new GameObject("AuraWeapon");
        TestAuraWeapon weapon = weaponObject.AddComponent<TestAuraWeapon>();

        GameObject auraObject = new GameObject("Aura");
        Aura aura = auraObject.AddComponent<Aura>();
        weapon.SetCurrentAura(aura);

        weapon.OnUnequip();

        yield return null;

        Assert.IsTrue(aura == null);
    }

    [UnityTest]
    public IEnumerator DoLevelUp_WhenCurrentAuraExists_ShouldResizeAuraAndReturnTrue()
    {
        GameObject weaponObject = new GameObject("AuraWeapon");
        TestAuraWeapon weapon = weaponObject.AddComponent<TestAuraWeapon>();
        weapon.enabled = false;

        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
        data.maxLevel = 2;
        data.baseStats = new Weapon.Stats
        {
            area = 1f
        };
        data.linearGrowth = new Weapon.Stats[]
        {
        new Weapon.Stats
        {
            area = 2f
        }
        };
        data.randomGrowth = new Weapon.Stats[0];

        weapon.data = data;
        weapon.maxLevel = data.maxLevel;
        weapon.currentLevel = 1;
        weapon.SetCurrentStats(data.baseStats);

        GameObject auraObject = new GameObject("Aura");
        Aura aura = auraObject.AddComponent<Aura>();
        aura.transform.localScale = Vector3.one;
        weapon.SetCurrentAura(aura);

        bool result = weapon.DoLevelUp();

        yield return null;

        Assert.IsTrue(result);
        Assert.AreEqual(2, weapon.currentLevel);
        Assert.AreEqual(new Vector3(3f, 3f, 3f), aura.transform.localScale);
    }
}