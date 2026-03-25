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

        GameObject playerObject = new GameObject("Player");
        PlayerMovement playerMovement = playerObject.AddComponent<PlayerMovement>();
        playerMovement.enabled = false;

        GameObject enemyObject = new GameObject("Enemy");
        enemyObject.tag = "Enemy";
        enemyObject.transform.position = Vector3.zero;
        enemyObject.AddComponent<SpriteRenderer>();
        enemyObject.AddComponent<EnemyMovement>();

        CircleCollider2D collider = enemyObject.AddComponent<CircleCollider2D>();
        collider.radius = 0.5f;

        EnemyStats enemy = enemyObject.AddComponent<EnemyStats>();
        enemy.currentHealth = 10f;
        enemy.currentDamage = 1f;
        enemy.currentMoveSpeed = 1f;

        yield return null;

        weapon.CallDamageArea(Vector2.zero, 5f, 3f);

        yield return null;

        Assert.AreEqual(7f, enemy.currentHealth);
    }
}