using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BloodyTearProjectilePlayModeTests
{
    private class TestWeapon : Weapon
    {
        public Stats stats;
        public float damage = 2f;

        protected override void Update()
        {
        }

        public override Stats GetStats()
        {
            return stats;
        }

        public override float GetDamage()
        {
            return damage;
        }

        public override float GetArea()
        {
            return stats.area;
        }

        public override float GetSpeed()
        {
            return stats.speed;
        }

        public override float GetLifespan()
        {
            return stats.lifespan;
        }
    }

    private class TestBloodyTearProjectile : BloodyTearProjectile
    {
        public void CallBaseStart()
        {
            base.Start();
        }

        public void CallOnTriggerEnter2D(Collider2D collider)
        {
            typeof(BloodyTearProjectile)
                .GetMethod("OnTriggerEnter2D", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(this, new object[] { collider });
        }
    }

    private PlayerStats CreateOwner(float currentHealth)
    {
        GameObject playerObject = new GameObject("Player");
        playerObject.SetActive(false);

        PlayerStats owner = playerObject.AddComponent<PlayerStats>();
        owner.enabled = false;

        CharacterData characterData = ScriptableObject.CreateInstance<CharacterData>();
        CharacterData.Stats stats = new CharacterData.Stats
        {
            maxHealth = 20f,
            might = 1f,
            area = 1f,
            speed = 1f,
            duration = 1f,
            cooldown = 1f,
            luck = 1f
        };

        owner.baseStats = stats;
        owner.Stats = stats;
        owner.CurrentHealth = currentHealth;

        typeof(PlayerStats)
            .GetField("characterData", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(owner, characterData);

        typeof(EntityStats)
            .GetField("health", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(owner, currentHealth);

        return owner;
    }

    private TestWeapon CreateWeapon(PlayerStats owner, bool criticalHit)
    {
        GameObject weaponObject = new GameObject("BloodyTearWeapon");
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();
        weapon.enabled = false;
        weapon.criticalHit = criticalHit;
        weapon.damage = 2f;
        weapon.stats = new Weapon.Stats
        {
            area = 1f,
            piercing = 2,
            lifespan = 0f,
            knockback = 0f
        };

        typeof(Item)
            .GetField("owner", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(weapon, owner);

        return weapon;
    }

    private EnemyStats CreateEnemy(out Collider2D enemyCollider)
    {
        GameObject enemyObject = new GameObject("Enemy");
        enemyObject.tag = "Enemy";
        enemyObject.AddComponent<SpriteRenderer>();
        enemyCollider = enemyObject.AddComponent<BoxCollider2D>();

        Rigidbody2D enemyBody = enemyObject.AddComponent<Rigidbody2D>();
        enemyBody.gravityScale = 0f;

        EnemyStats enemy = enemyObject.AddComponent<EnemyStats>();
        enemy.baseStats = new EnemyStats.Stats
        {
            maxHealth = 10f,
            moveSpeed = 1f,
            damage = 1f,
            knockbackMultiplier = 1f,
            resistances = new EnemyStats.Resitances()
        };

        return enemy;
    }

    private TestBloodyTearProjectile CreateProjectile(PlayerStats owner, TestWeapon weapon)
    {
        GameObject projectileObject = new GameObject("BloodyTearProjectile");

        Rigidbody2D body = projectileObject.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;

        TestBloodyTearProjectile projectile = projectileObject.AddComponent<TestBloodyTearProjectile>();
        projectile.owner = owner;
        projectile.weapon = weapon;
        projectile.CallBaseStart();

        return projectile;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        foreach (GameObject obj in Object.FindObjectsByType<GameObject>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        ))
        {
            Object.Destroy(obj);
        }

        yield return null;

        TestScriptableObjectCleanup.DestroyRuntimeObjects<CharacterData>();
        EnemyStats.count = 0;
    }

    [UnityTest]
    public IEnumerator OnTriggerEnter2D_WhenCriticalHit_ShouldHealOwnerAfterDamage()
    {
        PlayerStats owner = CreateOwner(4f);
        TestWeapon weapon = CreateWeapon(owner, true);
        TestBloodyTearProjectile projectile = CreateProjectile(owner, weapon);
        CreateEnemy(out Collider2D enemyCollider);

        yield return null;

        projectile.CallOnTriggerEnter2D(enemyCollider);

        Assert.AreEqual(20f, owner.CurrentHealth);
    }

    [UnityTest]
    public IEnumerator OnTriggerEnter2D_WhenNotCriticalHit_ShouldNotHealOwner()
    {
        PlayerStats owner = CreateOwner(4f);
        TestWeapon weapon = CreateWeapon(owner, false);
        TestBloodyTearProjectile projectile = CreateProjectile(owner, weapon);
        CreateEnemy(out Collider2D enemyCollider);

        yield return null;

        projectile.CallOnTriggerEnter2D(enemyCollider);

        Assert.AreEqual(4f, owner.CurrentHealth);
    }
}
