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

    private void SetPrivateField(object target, string fieldName, object value)
    {
        target.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(target, value);
    }

    private PlayerStats CreatePlayer()
    {
        GameObject playerObject = new GameObject("Player");
        playerObject.SetActive(false);

        PlayerStats playerStats = playerObject.AddComponent<PlayerStats>();
        playerStats.enabled = false;

        CharacterData.Stats stats = new CharacterData.Stats
        {
            maxHealth = 20f,
            recovery = 1f,
            armor = 0f,
            moveSpeed = 5f,
            might = 1f,
            area = 1f,
            speed = 1f,
            duration = 1f,
            amount = 0,
            cooldown = 1f,
            luck = 1f,
            growth = 1f,
            greed = 1f,
            curse = 0f,
            magnet = 1f,
            revival = 0
        };

        playerStats.baseStats = stats;
        playerStats.Stats = stats;
        playerStats.CurrentHealth = 20f;

        return playerStats;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.Destroy(go);
        }

        yield return null;

        foreach (var data in Resources.FindObjectsOfTypeAll<CharacterData>())
        {
            Object.DestroyImmediate(data, true);
        }
    }

    [UnityTest]
    public IEnumerator Update_WhenEnemyInside_ShouldDealDamage()
    {
        PlayerStats owner = CreatePlayer();

        GameObject auraObject = new GameObject("Aura");
        TestAura aura = auraObject.AddComponent<TestAura>();
        aura.enabled = false;
        aura.owner = owner;

        GameObject weaponObject = new GameObject("Weapon");
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();
        weapon.enabled = false;
        weapon.stats = new Weapon.Stats
        {
            cooldown = 0f,
            knockback = 0f
        };
        weapon.damage = 5f;
        aura.weapon = weapon;

        GameObject enemyObject = new GameObject("Enemy");
        SpriteRenderer spriteRenderer = enemyObject.AddComponent<SpriteRenderer>();
        spriteRenderer.color = Color.white;

        EnemyMovement enemyMovement = enemyObject.AddComponent<EnemyMovement>();
        enemyMovement.enabled = false;

        EnemyStats enemy = enemyObject.AddComponent<EnemyStats>();
        enemy.enabled = false;
        enemy.currentHealth = 10f;
        enemy.currentDamage = 1f;
        enemy.currentMoveSpeed = 1f;

        SetPrivateField(enemy, "spriteRenderer", spriteRenderer);
        SetPrivateField(enemy, "originalColor", Color.white);
        SetPrivateField(enemy, "enemyMovement", enemyMovement);

        CircleCollider2D enemyCollider = enemyObject.AddComponent<CircleCollider2D>();

        aura.CallOnTriggerEnter2D(enemyCollider);
        aura.CallUpdate();

        yield return null;

        Assert.AreEqual(5f, enemy.currentHealth);
    }

    [UnityTest]
    public IEnumerator OnTriggerExit2D_WhenEnemyLeaves_ShouldStopAffectingAfterNextTick()
    {
        PlayerStats owner = CreatePlayer();

        GameObject auraObject = new GameObject("Aura");
        TestAura aura = auraObject.AddComponent<TestAura>();
        aura.enabled = false;
        aura.owner = owner;

        GameObject weaponObject = new GameObject("Weapon");
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();
        weapon.enabled = false;
        weapon.stats = new Weapon.Stats
        {
            cooldown = 0f,
            knockback = 0f
        };
        weapon.damage = 3f;
        aura.weapon = weapon;

        GameObject enemyObject = new GameObject("Enemy");
        SpriteRenderer spriteRenderer = enemyObject.AddComponent<SpriteRenderer>();
        spriteRenderer.color = Color.white;

        EnemyMovement enemyMovement = enemyObject.AddComponent<EnemyMovement>();
        enemyMovement.enabled = false;

        EnemyStats enemy = enemyObject.AddComponent<EnemyStats>();
        enemy.enabled = false;
        enemy.currentHealth = 10f;
        enemy.currentDamage = 1f;
        enemy.currentMoveSpeed = 1f;

        SetPrivateField(enemy, "spriteRenderer", spriteRenderer);
        SetPrivateField(enemy, "originalColor", Color.white);
        SetPrivateField(enemy, "enemyMovement", enemyMovement);

        CircleCollider2D enemyCollider = enemyObject.AddComponent<CircleCollider2D>();

        aura.CallOnTriggerEnter2D(enemyCollider);
        aura.CallOnTriggerExit2D(enemyCollider);
        aura.CallUpdate();
        aura.CallUpdate();

        yield return null;

        Assert.AreEqual(10f, enemy.currentHealth);
    }
}