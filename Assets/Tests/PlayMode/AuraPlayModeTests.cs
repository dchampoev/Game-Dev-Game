using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class AuraPlayModeTests
{
    private class TestAura : Aura
    {
        public void CallUpdate()
        {
            typeof(Aura)
                .GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(this, null);
        }

        public void CallOnTriggerEnter2D(Collider2D collider)
        {
            typeof(Aura)
                .GetMethod("OnTriggerEnter2D", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(this, new object[] { collider });
        }

        public void CallOnTriggerExit2D(Collider2D collider)
        {
            typeof(Aura)
                .GetMethod("OnTriggerExit2D", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(this, new object[] { collider });
        }
    }

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
    public IEnumerator Update_WhenEnemyInside_ShouldDealDamage()
    {
        GameObject playerObject = new GameObject("Player");
        PlayerMovement playerMovement = playerObject.AddComponent<PlayerMovement>();
        playerMovement.enabled = false;

        GameObject auraObject = new GameObject("Aura");
        TestAura aura = auraObject.AddComponent<TestAura>();
        aura.enabled = false;

        GameObject weaponObject = new GameObject("Weapon");
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();
        weapon.stats = new Weapon.Stats
        {
            cooldown = 0f,
            knockback = 0f
        };
        weapon.damage = 5f;
        aura.weapon = weapon;

        GameObject enemyObject = new GameObject("Enemy");
        enemyObject.AddComponent<SpriteRenderer>();

        EnemyMovement enemyMovement = enemyObject.AddComponent<EnemyMovement>();
        enemyMovement.enabled = false;

        EnemyStats enemy = enemyObject.AddComponent<EnemyStats>();
        enemy.enabled = false;
        enemy.currentHealth = 10f;
        enemy.currentDamage = 1f;
        enemy.currentMoveSpeed = 1f;

        CircleCollider2D enemyCollider = enemyObject.AddComponent<CircleCollider2D>();

        aura.CallOnTriggerEnter2D(enemyCollider);

        LogAssert.Expect(LogType.Exception, "NullReferenceException: Object reference not set to an instance of an object");
        aura.CallUpdate();

        yield return null;

        Assert.AreEqual(5f, enemy.currentHealth);
    }

    [UnityTest]
    public IEnumerator OnTriggerExit2D_WhenEnemyLeaves_ShouldStopAffectingAfterNextTick()
    {
        GameObject playerObject = new GameObject("Player");
        PlayerMovement playerMovement = playerObject.AddComponent<PlayerMovement>();
        playerMovement.enabled = false;

        GameObject auraObject = new GameObject("Aura");
        TestAura aura = auraObject.AddComponent<TestAura>();
        aura.enabled = false;

        GameObject weaponObject = new GameObject("Weapon");
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();
        weapon.stats = new Weapon.Stats
        {
            cooldown = 0f,
            knockback = 0f
        };
        weapon.damage = 3f;
        aura.weapon = weapon;

        GameObject enemyObject = new GameObject("Enemy");
        enemyObject.AddComponent<SpriteRenderer>();

        EnemyMovement enemyMovement = enemyObject.AddComponent<EnemyMovement>();
        enemyMovement.enabled = false;

        EnemyStats enemy = enemyObject.AddComponent<EnemyStats>();
        enemy.enabled = false;
        enemy.currentHealth = 10f;
        enemy.currentDamage = 1f;
        enemy.currentMoveSpeed = 1f;

        CircleCollider2D enemyCollider = enemyObject.AddComponent<CircleCollider2D>();

        aura.CallOnTriggerEnter2D(enemyCollider);
        aura.CallOnTriggerExit2D(enemyCollider);
        aura.CallUpdate();
        aura.CallUpdate();

        yield return null;

        Assert.AreEqual(10f, enemy.currentHealth);
    }
}