using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class ProjectilePropsTests
{
    private class TestWeapon : Weapon
    {
        public Stats stats;
        public float damage = 5f;

        public override Stats GetStats() => stats;
        public override float GetDamage() => damage;
        public override float GetArea() => stats.area;
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

        public int GetPiercing()
        {
            return (int)typeof(Projectile)
                .GetField("piercing", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(this);
        }
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(go);
        }
    }

    [Test]
    public void OnTriggerEnter2D_WhenHittingProps_ShouldDamageAndReducePiercing()
    {
        GameObject weaponObject = new GameObject("Weapon");
        TestWeapon weapon = weaponObject.AddComponent<TestWeapon>();
        weapon.enabled = false;
        weapon.stats = new Weapon.Stats
        {
            speed = 0f,
            area = 1f,
            piercing = 2,
            lifespan = 0f,
            hitEffect = null
        };

        GameObject projectileObject = new GameObject("Projectile");
        Rigidbody2D rb = projectileObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        TestProjectile projectile = projectileObject.AddComponent<TestProjectile>();
        projectile.enabled = false;
        projectile.weapon = weapon;
        projectile.CallStart();

        GameObject propsObject = new GameObject("Props");
        BoxCollider2D collider = propsObject.AddComponent<BoxCollider2D>();
        BreakableProps props = propsObject.AddComponent<BreakableProps>();
        props.health = 10f;

        projectile.CallOnTriggerEnter2D(collider);

        Assert.AreEqual(5f, props.health);
        Assert.AreEqual(1, projectile.GetPiercing());
    }
}