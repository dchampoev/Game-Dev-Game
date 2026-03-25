using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ProjectileTests
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

        public void CallFixedUpdate()
        {
            typeof(Projectile)
                .GetMethod("FixedUpdate", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(this, null);
        }

        public void CallOnTriggerEnter2D(Collider2D collider)
        {
            typeof(Projectile)
                .GetMethod("OnTriggerEnter2D", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(this, new object[] { collider });
        }
    }

    private void SetProtectedField(object target, string fieldName, object value)
    {
        target.GetType()
            .BaseType
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(target, value);
    }

    [Test]
    public void AcquireAutoAimFacing_WhenNoEnemies_ShouldKeepRotationUnchanged()
    {
        GameObject projectileObject = new GameObject("Projectile");
        TestProjectile projectile = projectileObject.AddComponent<TestProjectile>();

        Quaternion before = projectile.transform.rotation;

        projectile.AcquireAutoAimFacing();

        Quaternion after = projectile.transform.rotation;

        Assert.AreEqual(before, after);
    }

    [Test]
    public void AcquireAutoAimFacing_WhenEnemyExists_ShouldFaceEnemy()
    {
        GameObject projectileObject = new GameObject("Projectile");
        TestProjectile projectile = projectileObject.AddComponent<TestProjectile>();
        projectile.transform.position = Vector3.zero;

        GameObject enemyObject = new GameObject("Enemy");
        enemyObject.transform.position = new Vector3(10f, 0f, 0f);
        enemyObject.AddComponent<EnemyStats>();

        projectile.AcquireAutoAimFacing();

        float z = projectile.transform.eulerAngles.z;

        Assert.That(Mathf.Abs(Mathf.DeltaAngle(z, 0f)), Is.LessThan(0.1f));
    }

    [Test]
    public void Start_WhenKinematicBody_ShouldSetScaleAndPiercing()
    {
        GameObject weaponObject = new GameObject("Weapon");
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();
        weapon.stats = new Weapon.Stats
        {
            speed = 5f,
            area = 2f,
            piercing = 3,
            lifespan = 0f
        };

        GameObject projectileObject = new GameObject("Projectile");
        projectileObject.transform.localScale = new Vector3(1f, 1f, 1f);

        Rigidbody2D rb = projectileObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        TestProjectile projectile = projectileObject.AddComponent<TestProjectile>();
        projectile.weapon = weapon;

        projectile.CallStart();

        Assert.AreEqual(2f, projectile.transform.localScale.x);
        Assert.AreEqual(2f, projectile.transform.localScale.y);

        int piercing = (int)typeof(Projectile)
            .GetField("piercing", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(projectile);

        Assert.AreEqual(3, piercing);
    }

    [Test]
    public void FixedUpdate_WhenBodyIsKinematic_ShouldMoveProjectileForward()
    {
        GameObject weaponObject = new GameObject("Weapon");
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();
        weapon.stats = new Weapon.Stats
        {
            speed = 10f,
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

        Vector3 before = projectile.transform.position;

        projectile.CallFixedUpdate();

        Vector3 after = projectile.transform.position;

        Assert.Greater(after.x, before.x);
    }
}