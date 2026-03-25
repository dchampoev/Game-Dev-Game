using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ProjectilePlayModeTests
{
    private class TestWeapon : Weapon
    {
        public Stats stats;
        public float damage = 2f;

        public override Stats GetStats()
        {
            return stats;
        }

        public override float GetDamage()
        {
            return damage;
        }
    }

    private class TestProjectile : Projectile
    {
        public void CallStart()
        {
            typeof(Projectile)
                .GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(this, null);
        }

        public void CallOnTriggerEnter2D(Collider2D collider)
        {
            typeof(Projectile)
                .GetMethod("OnTriggerEnter2D", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(this, new object[] { collider });
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
    public IEnumerator OnTriggerEnter2D_WhenEnemyHitAndPiercingBecomesZero_ShouldDamageEnemyAndDestroyProjectile()
    {
        GameObject playerObject = new GameObject("Player");
        playerObject.tag = "Player";
        playerObject.AddComponent<BoxCollider2D>();

        PlayerMovement playerMovement = playerObject.AddComponent<PlayerMovement>();
        playerMovement.enabled = false;

        PlayerStats playerStats = playerObject.AddComponent<PlayerStats>();
        playerStats.enabled = false;

        GameObject weaponObject = new GameObject("Weapon");
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();
        weapon.damage = 2f;
        weapon.stats = new Weapon.Stats
        {
            speed = 0f,
            area = 1f,
            piercing = 1,
            lifespan = 0f
        };

        GameObject projectileObject = new GameObject("Projectile");
        Rigidbody2D rb = projectileObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        TestProjectile projectile = projectileObject.AddComponent<TestProjectile>();
        projectile.weapon = weapon;
        projectile.CallStart();

        GameObject enemyObject = new GameObject("Enemy");
        enemyObject.tag = "Enemy";
        BoxCollider2D enemyCollider = enemyObject.AddComponent<BoxCollider2D>();
        enemyObject.AddComponent<SpriteRenderer>();

        EnemyMovement enemyMovement = enemyObject.AddComponent<EnemyMovement>();
        EnemyStats enemyStats = enemyObject.AddComponent<EnemyStats>();

        enemyStats.currentHealth = 10f;
        enemyStats.currentDamage = 1f;
        enemyStats.currentMoveSpeed = 1f;

        yield return null;

        projectile.CallOnTriggerEnter2D(enemyCollider);

        yield return null;

        Assert.AreEqual(8f, enemyStats.currentHealth);
        Assert.IsTrue(projectileObject == null);
    }
}