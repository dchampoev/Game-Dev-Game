using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;
using System.Reflection;

public class ProjectileWeaponBehaviourPlayModeTests
{
    private class TestProjectileWeaponBehaviour : ProjectileWeaponBehaviour
    {
        public void CallOnTriggerEnter2D(Collider2D collider)
        {
            typeof(ProjectileWeaponBehaviour)
                .GetMethod("OnTriggerEnter2D", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(this, new object[] { collider });
        }
    }

    private WeaponScriptableObject CreateWeaponData(
        float damage = 2f,
        float speed = 5f,
        float cooldown = 1f,
        int pierce = 1)
    {
        WeaponScriptableObject data = ScriptableObject.CreateInstance<WeaponScriptableObject>();
        SerializedObject so = new SerializedObject(data);
        so.FindProperty("damage").floatValue = damage;
        so.FindProperty("speed").floatValue = speed;
        so.FindProperty("cooldownDuration").floatValue = cooldown;
        so.FindProperty("pierce").intValue = pierce;
        so.ApplyModifiedProperties();
        return data;
    }

    [UnityTest]
    public IEnumerator OnTriggerEnter2D_WhenEnemyAndPierceBecomesZero_ShouldDestroyProjectile()
    {
        GameObject projectileObject = new GameObject("Projectile");
        TestProjectileWeaponBehaviour behaviour = projectileObject.AddComponent<TestProjectileWeaponBehaviour>();

        behaviour.weaponData = CreateWeaponData(damage: 2f, speed: 1f, cooldown: 1f, pierce: 1);
        behaviour.InitializeStats();

        GameObject enemyObject = new GameObject("Enemy");
        enemyObject.tag = "Enemy";
        BoxCollider2D enemyCollider = enemyObject.AddComponent<BoxCollider2D>();

        EnemyStats enemyStats = enemyObject.AddComponent<EnemyStats>();
        enemyStats.enabled = false;
        enemyStats.currentHealth = 10f;

        behaviour.CallOnTriggerEnter2D(enemyCollider);

        yield return null;

        Assert.IsTrue(projectileObject == null);
        Assert.AreEqual(8f, enemyStats.currentHealth);

        Object.Destroy(enemyObject);
        Object.Destroy(behaviour.weaponData);
    }
}