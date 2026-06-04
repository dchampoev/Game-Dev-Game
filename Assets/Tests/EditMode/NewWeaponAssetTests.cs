#if UNITY_EDITOR
using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class NewWeaponAssetTests
{
    private static readonly object[] NewWeaponCases =
    {
        new object[] { "Cross", "ProjectileWeapon", typeof(Projectile), true },
        new object[] { "Bloody Tear", "WhipWeapon", typeof(BloodyTearProjectile), false },
        new object[] { "Thousand Edge", "ProjectileWeapon", typeof(Projectile), false },
        new object[] { "Unholy Vespers", "UnholyVespersWeapon", typeof(KingBibleProjectile), false }
    };

    private static readonly object[] AreaWeaponCases =
    {
        new object[] { "Thunder Loop", "ThunderLoopWeapon" }
    };

    [TestCaseSource(nameof(NewWeaponCases))]
    public void NewWeaponAssets_ShouldResolveBehaviourAndHaveDamageProjectileSetup(
        string weaponName,
        string expectedBehaviour,
        Type expectedProjectileType,
        bool expectedAutoAim)
    {
        WeaponData data = AssetDatabase.LoadAssetAtPath<WeaponData>(
            $"Assets/Scriptable Objects/Weapons/{weaponName}.asset"
        );

        Assert.NotNull(data, $"{weaponName} asset is missing.");
        Assert.AreEqual(expectedBehaviour, data.behaviour);

        Type behaviourType = typeof(Weapon).Assembly.GetType(data.behaviour);
        Assert.NotNull(behaviourType, $"{weaponName} behaviour does not resolve in the gameplay assembly.");
        Assert.IsTrue(typeof(Weapon).IsAssignableFrom(behaviourType));

        Projectile projectilePrefab = data.baseStats.projectilePrefab;
        Assert.NotNull(projectilePrefab, $"{weaponName} does not have a projectile prefab.");
        Assert.IsTrue(
            expectedProjectileType.IsAssignableFrom(projectilePrefab.GetType()),
            $"{weaponName} should use {expectedProjectileType.Name} or a subclass."
        );

        Collider2D collider = projectilePrefab.GetComponent<Collider2D>();
        Assert.NotNull(collider, $"{weaponName} projectile needs a Collider2D so it can hit enemies.");
        Assert.IsTrue(collider.isTrigger, $"{weaponName} projectile collider must be a trigger.");

        Rigidbody2D body = projectilePrefab.GetComponent<Rigidbody2D>();
        Assert.NotNull(body, $"{weaponName} projectile needs a Rigidbody2D for trigger callbacks.");

        if (expectedAutoAim)
        {
            Assert.IsTrue(projectilePrefab.hasAutoAim);
        }
    }

    [TestCaseSource(nameof(AreaWeaponCases))]
    public void AreaWeaponAssets_ShouldResolveBehaviourAndHaveHitEffectSetup(
        string weaponName,
        string expectedBehaviour)
    {
        WeaponData data = AssetDatabase.LoadAssetAtPath<WeaponData>(
            $"Assets/Scriptable Objects/Weapons/{weaponName}.asset"
        );

        Assert.NotNull(data, $"{weaponName} asset is missing.");
        Assert.AreEqual(expectedBehaviour, data.behaviour);

        Type behaviourType = typeof(Weapon).Assembly.GetType(data.behaviour);
        Assert.NotNull(behaviourType, $"{weaponName} behaviour does not resolve in the gameplay assembly.");
        Assert.IsTrue(typeof(Weapon).IsAssignableFrom(behaviourType));

        Assert.NotNull(data.baseStats.hitEffect, $"{weaponName} needs a hit VFX prefab.");
        Assert.Greater(data.baseStats.damage, 0f, $"{weaponName} should deal damage.");
        Assert.Greater(data.baseStats.area, 0f, $"{weaponName} should have an area radius.");
        Assert.Greater(data.baseStats.number, 0, $"{weaponName} should strike at least once.");
    }
}
#endif
