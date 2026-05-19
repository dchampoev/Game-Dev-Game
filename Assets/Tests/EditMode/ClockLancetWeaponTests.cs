using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class ClockLancetWeaponTests
{
    private class TestProjectile : Projectile
    {
        protected override void Start()
        {
        }

        protected override void FixedUpdate()
        {
        }
    }

    private class TestClockLancetWeapon : ClockLancetWeapon
    {
        public bool CallAttack(int attackCount = 1)
        {
            return (bool)typeof(ClockLancetWeapon)
                .GetMethod("Attack", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(this, new object[] { attackCount });
        }

        public float CallGetSpawnAngle()
        {
            return (float)typeof(ClockLancetWeapon)
                .GetMethod("GetSpawnAngle", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(this, null);
        }

        public float CurrentAngle
        {
            get
            {
                return (float)typeof(ClockLancetWeapon)
                    .GetField("currentAngle", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(this);
            }
            set
            {
                typeof(ClockLancetWeapon)
                    .GetField("currentAngle", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(this, value);
            }
        }

        public int CurrentAttackCount
        {
            get
            {
                return (int)typeof(ProjectileWeapon)
                    .GetField("currentAttackCount", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(this);
            }
        }

        public void SetOwner(PlayerStats value)
        {
            typeof(Item)
                .GetField("owner", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(this, value);
        }

        public void SetCurrentStats(Weapon.Stats value)
        {
            typeof(Weapon)
                .GetField("currentStats", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(this, value);
        }

        public void SetCurrentCooldown(float value)
        {
            typeof(Weapon)
                .GetField("currentCooldown", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(this, value);
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
            cooldown = 1f
        };
        return owner;
    }

    private TestClockLancetWeapon CreateWeapon()
    {
        PlayerStats owner = CreateOwner();

        GameObject projectileObject = new GameObject("ClockLancetProjectile");
        TestProjectile projectilePrefab = projectileObject.AddComponent<TestProjectile>();

        GameObject weaponObject = new GameObject("ClockLancet");
        weaponObject.SetActive(false);
        TestClockLancetWeapon weapon = weaponObject.AddComponent<TestClockLancetWeapon>();
        weapon.SetOwner(owner);
        weapon.SetCurrentCooldown(0f);
        weapon.SetCurrentStats(new Weapon.Stats
        {
            projectilePrefab = projectilePrefab,
            speed = 1f,
            area = 1f,
            cooldown = 1f,
            piercing = 1,
            number = 1,
            spawnVariance = new Rect(0f, 0f, 0f, 0f)
        });
        return weapon;
    }

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(obj);
        }
    }

    [Test]
    public void GetSpawnAngle_ShouldUseCurrentClockLancetAngle()
    {
        TestClockLancetWeapon weapon = CreateWeapon();
        weapon.CurrentAngle = 30f;

        float result = weapon.CallGetSpawnAngle();

        Assert.AreEqual(30f, result);
    }

    [Test]
    public void Attack_WhenSuccessful_ShouldAdvanceAngleByOneClockStep()
    {
        TestClockLancetWeapon weapon = CreateWeapon();

        bool result = weapon.CallAttack();

        Assert.IsTrue(result);
        Assert.AreEqual(60f, weapon.CurrentAngle);
    }

    [Test]
    public void Attack_WhenAngleMovesPastNegativeOneEighty_ShouldWrapIntoPositiveRange()
    {
        TestClockLancetWeapon weapon = CreateWeapon();
        weapon.CurrentAngle = -180f;

        bool result = weapon.CallAttack();

        Assert.IsTrue(result);
        Assert.AreEqual(150f, weapon.CurrentAngle);
    }

    [Test]
    public void Attack_WhenAskedForMultipleAttacks_ShouldOnlyQueueSingleClockLancetShot()
    {
        TestClockLancetWeapon weapon = CreateWeapon();

        bool result = weapon.CallAttack(3);

        Assert.IsTrue(result);
        Assert.AreEqual(0, weapon.CurrentAttackCount);
    }
}
