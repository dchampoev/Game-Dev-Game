using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ThunderLoopWeaponPlayModeTests
{
    private class TestThunderLoopWeapon : ThunderLoopWeapon
    {
        public IEnumerator CallStrike(EnemyStats target)
        {
            return (IEnumerator)typeof(ThunderLoopWeapon)
                .GetMethod("Strike", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(this, new object[] { target });
        }

        public void CallDamageArea(Vector2 position, float radius, float damage)
        {
            typeof(ThunderLoopWeapon)
                .GetMethod("DamageArea", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(this, new object[] { position, radius, damage });
        }

        public void SetCurrentStats(Weapon.Stats value)
        {
            typeof(Weapon)
                .GetField("currentStats", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(this, value);
        }

        public void SetOwner(PlayerStats value)
        {
            typeof(Item)
                .GetField("owner", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(this, value);
        }
    }

    private class TestEnemyMovement : EnemyMovement
    {
        public override void Knockback(Vector2 velocity, float duration)
        {
        }
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        foreach (GameObject obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.Destroy(obj);
        }

        yield return null;
    }

    [UnityTest]
    public IEnumerator DamageArea_WhenEnemyIsInsideRadius_ShouldDamageEnemy()
    {
        TestThunderLoopWeapon weapon = CreateWeapon(3f, 5f);
        EnemyStats enemy = CreateEnemy(Vector2.zero, 10f);

        yield return new WaitForFixedUpdate();

        weapon.CallDamageArea(Vector2.zero, 5f, 3f);

        yield return null;

        Assert.AreEqual(7f, GetCurrentHealth(enemy));
    }

    [UnityTest]
    public IEnumerator Strike_WhenEnemyIsValid_ShouldDamageTwice()
    {
        TestThunderLoopWeapon weapon = CreateWeapon(3f, 5f);
        EnemyStats enemy = CreateEnemy(Vector2.zero, 20f);

        yield return new WaitForFixedUpdate();

        yield return weapon.CallStrike(enemy);

        Assert.AreEqual(14f, GetCurrentHealth(enemy));
    }

    [UnityTest]
    public IEnumerator Strike_WhenTargetIsNull_ShouldExitWithoutException()
    {
        TestThunderLoopWeapon weapon = CreateWeapon(3f, 5f);

        yield return weapon.CallStrike(null);

        Assert.Pass();
    }

    private static TestThunderLoopWeapon CreateWeapon(float damage, float area)
    {
        GameObject weaponObject = new GameObject("ThunderLoop");
        TestThunderLoopWeapon weapon = weaponObject.AddComponent<TestThunderLoopWeapon>();
        weapon.enabled = false;
        weapon.SetOwner(CreateOwner());

        ParticleSystem hitEffect = new GameObject("ThunderLoopHitEffect").AddComponent<ParticleSystem>();
        weapon.SetCurrentStats(new Weapon.Stats
        {
            hitEffect = hitEffect,
            damage = damage,
            damageVariance = 0f,
            area = area,
            appliedBuffs = System.Array.Empty<EntityStats.BuffInfo>()
        });

        return weapon;
    }

    private static PlayerStats CreateOwner()
    {
        GameObject playerObject = new GameObject("Player");
        playerObject.SetActive(false);

        PlayerStats owner = playerObject.AddComponent<PlayerStats>();
        owner.enabled = false;
        owner.Stats = new CharacterData.Stats
        {
            might = 1f,
            area = 0f,
            duration = 1f,
            speed = 1f,
            cooldown = 1f,
            luck = 1f
        };

        return owner;
    }

    private static EnemyStats CreateEnemy(Vector2 position, float maxHealth)
    {
        GameObject enemyObject = new GameObject("Enemy");
        enemyObject.tag = "Enemy";
        enemyObject.transform.position = position;

        SpriteRenderer spriteRenderer = enemyObject.AddComponent<SpriteRenderer>();
        spriteRenderer.color = Color.white;

        TestEnemyMovement enemyMovement = enemyObject.AddComponent<TestEnemyMovement>();
        enemyMovement.enabled = false;

        CircleCollider2D collider = enemyObject.AddComponent<CircleCollider2D>();
        collider.radius = 0.5f;

        EnemyStats enemy = enemyObject.AddComponent<EnemyStats>();
        enemy.baseStats = new EnemyStats.Stats
        {
            maxHealth = maxHealth,
            moveSpeed = 1f,
            damage = 1f,
            knockbackMultiplier = 1f,
            resistances = new EnemyStats.Resistances()
        };

        typeof(EnemyStats)
            .GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(enemy, null);

        return enemy;
    }

    private static float GetCurrentHealth(EnemyStats enemy)
    {
        return (float)typeof(EntityStats)
            .GetField("health", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(enemy);
    }
}
