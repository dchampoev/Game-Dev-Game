using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class LightningRingWeaponPlayModeTests
{
    private class TestLightningRing : LightningRingWeapon
    {
        public void CallDamageArea(Vector2 position, float radius, float damage)
        {
            typeof(LightningRingWeapon)
                .GetMethod("DamageArea", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(this, new object[] { position, radius, damage });
        }
    }

    private class TestEnemyMovement : EnemyMovement
    {
        public override void Knockback(Vector2 velocity, float duration)
        {
        }
    }

    private void SetPrivateField(object target, string fieldName, object value)
    {
        target.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(target, value);
    }

    private float GetCurrentHealth(EnemyStats enemy)
    {
        return (float)typeof(EntityStats)
            .GetField("health", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(enemy);
    }

    private void CallStart(EnemyStats stats)
    {
        typeof(EnemyStats)
            .GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(stats, null);
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
    public IEnumerator DamageArea_WhenEnemyIsInsideRadius_ShouldDamageEnemy()
    {
        GameObject weaponObj = new GameObject("Weapon");
        TestLightningRing weapon = weaponObj.AddComponent<TestLightningRing>();
        weapon.enabled = false;

        GameObject enemyObject = new GameObject("Enemy");
        enemyObject.tag = "Enemy";
        enemyObject.transform.position = Vector3.zero;

        SpriteRenderer spriteRenderer = enemyObject.AddComponent<SpriteRenderer>();
        spriteRenderer.color = Color.white;

        TestEnemyMovement enemyMovement = enemyObject.AddComponent<TestEnemyMovement>();
        enemyMovement.enabled = false;

        CircleCollider2D collider = enemyObject.AddComponent<CircleCollider2D>();
        collider.radius = 0.5f;

        EnemyStats enemy = enemyObject.AddComponent<EnemyStats>();
        enemy.baseStats = new EnemyStats.Stats
        {
            maxHealth = 10f,
            moveSpeed = 1f,
            damage = 1f,
            knockbackMultiplier = 1f,
            resistances = new EnemyStats.Resitances()
        };

        CallStart(enemy);

        yield return new WaitForFixedUpdate();
        
        weapon.CallDamageArea(Vector2.zero, 5f, 3f);

        yield return null;

        Assert.AreEqual(7f, GetCurrentHealth(enemy));
    }
}
