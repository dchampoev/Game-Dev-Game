using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class KingBibleProjectilePlayModeTests
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

    private class TestKingBibleProjectile : KingBibleProjectile
    {
        public void CallStart()
        {
            base.Start();
        }

        public void CallFixedUpdate()
        {
            base.FixedUpdate();
        }

        public void CallUpdate()
        {
            typeof(KingBibleProjectile)
                .GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(this, null);
        }

        public void CallOnTriggerEnter2D(Collider2D collider)
        {
            typeof(KingBibleProjectile)
                .GetMethod("OnTriggerEnter2D", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(this, new object[] { collider });
        }

        public void SetPrivateState(float angle, Vector3 startScale, float startLifespan)
        {
            typeof(KingBibleProjectile)
                .GetField("angle", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(this, angle);

            typeof(KingBibleProjectile)
                .GetField("startScale", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(this, startScale);

            typeof(KingBibleProjectile)
                .GetField("startLifespan", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(this, startLifespan);
        }
    }

    private PlayerStats CreateOwner()
    {
        GameObject playerObject = new GameObject("Player");
        playerObject.SetActive(false);
        PlayerStats owner = playerObject.AddComponent<PlayerStats>();
        owner.enabled = false;
        owner.Stats = new CharacterData.Stats
        {
            might = 1f,
            area = 1f,
            speed = 1f,
            duration = 1f,
            cooldown = 1f
        };

        return owner;
    }

    private TestWeapon CreateWeapon(PlayerStats owner, float damage = 2f)
    {
        GameObject weaponObject = new GameObject("KingBibleWeapon");
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();
        weapon.enabled = false;
        weapon.damage = damage;
        weapon.stats = new Weapon.Stats
        {
            area = 2f,
            speed = 1f,
            lifespan = 3f,
            piercing = 3,
            knockback = 0f
        };

        typeof(Item)
            .GetField("owner", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(weapon, owner);

        return weapon;
    }

    private TestKingBibleProjectile CreateProjectile(PlayerStats owner, TestWeapon weapon, bool withRigidbody = true)
    {
        GameObject projectileObject = new GameObject("KingBibleProjectile");
        projectileObject.transform.position = owner.transform.position + Vector3.right * 2f;

        if (withRigidbody)
        {
            Rigidbody2D body = projectileObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
        }

        TestKingBibleProjectile projectile = projectileObject.AddComponent<TestKingBibleProjectile>();
        projectile.owner = owner;
        projectile.weapon = weapon;
        projectile.transitionTime = 0.01f;
        projectile.hitDelay = 0.5f;
        projectile.CallStart();

        return projectile;
    }

    private TestKingBibleProjectile CreateProjectileWithoutStart(PlayerStats owner, TestWeapon weapon)
    {
        GameObject projectileObject = new GameObject("KingBibleProjectile");
        projectileObject.SetActive(false);
        projectileObject.transform.position = owner.transform.position + Vector3.right * 2f;

        TestKingBibleProjectile projectile = projectileObject.AddComponent<TestKingBibleProjectile>();
        projectile.enabled = false;
        projectile.owner = owner;
        projectile.weapon = weapon;
        projectile.transitionTime = 0.01f;
        projectile.hitDelay = 0.5f;
        projectile.SetPrivateState(0f, new Vector3(2f, 2f, 1f), weapon.GetLifespan());

        return projectile;
    }

    private EnemyStats CreateEnemy(out Collider2D enemyCollider)
    {
        GameObject enemyObject = new GameObject("Enemy");
        enemyObject.tag = "Enemy";
        enemyObject.AddComponent<SpriteRenderer>();
        enemyCollider = enemyObject.AddComponent<BoxCollider2D>();

        Rigidbody2D body = enemyObject.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;

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

    private float GetHealth(EnemyStats enemy)
    {
        return (float)typeof(EntityStats)
            .GetField("health", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(enemy);
    }

    private int GetPiercing(KingBibleProjectile projectile)
    {
        return (int)typeof(Projectile)
            .GetField("piercing", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(projectile);
    }

    private int GetAffectedTargetCount(KingBibleProjectile projectile)
    {
        object affectedTargets = typeof(KingBibleProjectile)
            .GetField("affectedTargets", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(projectile);

        return (int)affectedTargets
            .GetType()
            .GetProperty("Count")
            .GetValue(affectedTargets);
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

        EnemyStats.count = 0;
        SpawnManager.instance = null;
    }

    [UnityTest]
    public IEnumerator Start_ShouldGrowFromZeroToScaledArea()
    {
        PlayerStats owner = CreateOwner();
        TestWeapon weapon = CreateWeapon(owner);

        TestKingBibleProjectile projectile = CreateProjectile(owner, weapon);

        Assert.AreEqual(Vector3.zero, projectile.transform.localScale);

        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForEndOfFrame();
        }

        Assert.That(projectile.transform.localScale.x, Is.GreaterThan(0f));
        Assert.That(projectile.transform.localScale.y, Is.GreaterThan(0f));
    }

    [UnityTest]
    public IEnumerator FixedUpdate_WhenKinematic_ShouldOrbitAroundOwner()
    {
        PlayerStats owner = CreateOwner();
        TestWeapon weapon = CreateWeapon(owner);
        TestKingBibleProjectile projectile = CreateProjectile(owner, weapon);

        projectile.CallFixedUpdate();

        yield return new WaitForFixedUpdate();

        Assert.That(projectile.transform.position.x, Is.EqualTo(2.2f).Within(0.05f));
        Assert.That(projectile.transform.position.y, Is.EqualTo(0f).Within(0.05f));
    }

    [UnityTest]
    public IEnumerator Update_WhenNoRigidbody_ShouldMoveProjectileAroundOwner()
    {
        PlayerStats owner = CreateOwner();
        TestWeapon weapon = CreateWeapon(owner);
        TestKingBibleProjectile projectile = CreateProjectileWithoutStart(owner, weapon);

        projectile.CallUpdate();

        Assert.That(projectile.transform.position.x, Is.EqualTo(2.2f).Within(0.05f));
        Assert.That(projectile.transform.position.y, Is.EqualTo(0f).Within(0.05f));

        yield return null;
    }

    [UnityTest]
    public IEnumerator HitDelay_WhenEnemyEntered_ShouldDamageOnceThenWaitForDelay()
    {
        PlayerStats owner = CreateOwner();
        TestWeapon weapon = CreateWeapon(owner, 2f);
        TestKingBibleProjectile projectile = CreateProjectile(owner, weapon);
        EnemyStats enemy = CreateEnemy(out Collider2D enemyCollider);

        yield return null;

        projectile.CallOnTriggerEnter2D(enemyCollider);
        projectile.HitDelay();

        Assert.AreEqual(8f, GetHealth(enemy));
        Assert.AreEqual(2, GetPiercing(projectile));
        Assert.AreEqual(1, GetAffectedTargetCount(projectile));

        projectile.HitDelay();

        Assert.AreEqual(8f, GetHealth(enemy));
        Assert.AreEqual(2, GetPiercing(projectile));
    }

    [UnityTest]
    public IEnumerator OnTriggerEnter2D_WhenHittingBreakableProp_ShouldDamagePropAndReducePiercing()
    {
        PlayerStats owner = CreateOwner();
        TestWeapon weapon = CreateWeapon(owner, 4f);
        TestKingBibleProjectile projectile = CreateProjectile(owner, weapon);

        GameObject propObject = new GameObject("Prop");
        Collider2D propCollider = propObject.AddComponent<BoxCollider2D>();
        BreakableProps prop = propObject.AddComponent<BreakableProps>();
        prop.health = 10f;

        projectile.CallOnTriggerEnter2D(propCollider);

        Assert.AreEqual(6f, prop.health);
        Assert.AreEqual(2, GetPiercing(projectile));

        yield return null;
    }
}
