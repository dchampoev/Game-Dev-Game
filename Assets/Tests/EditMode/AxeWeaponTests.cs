using NUnit.Framework;
using UnityEngine;

public class AxeWeaponTests
{
    private class TestAxeWeapon : AxeWeapon
    {
        public void SetCurrentStats(Weapon.Stats stats)
        {
            currentStats = stats;
        }

        public void SetCurrentAttackCount(int count)
        {
            currentAttackCount = count;
        }

        public void SetMovement(PlayerMovement playerMovement)
        {
            movement = playerMovement;
        }

        public float CallGetSpawnAngle()
        {
            return GetSpawnAngle();
        }

        public Vector2 CallGetSpawnOffset()
        {
            return GetSpawnOffset();
        }
    }

    [Test]
    public void GetSpawnAngle_WhenMovingRight_ShouldReturnCorrectAngle()
    {
        GameObject go = new GameObject();
        TestAxeWeapon weapon = go.AddComponent<TestAxeWeapon>();

        weapon.SetCurrentStats(new Weapon.Stats
        {
            number = 3
        });

        weapon.SetCurrentAttackCount(1);

        PlayerMovement movement = go.AddComponent<PlayerMovement>();
        movement.lastMoveDirection = Vector2.right;
        weapon.SetMovement(movement);

        float angle = weapon.CallGetSpawnAngle();

        Assert.AreEqual(80f, angle);
    }

    [Test]
    public void GetSpawnAngle_WhenMovingLeft_ShouldFlipAngle()
    {
        GameObject go = new GameObject();
        TestAxeWeapon weapon = go.AddComponent<TestAxeWeapon>();

        weapon.SetCurrentStats(new Weapon.Stats
        {
            number = 3
        });

        weapon.SetCurrentAttackCount(1);

        PlayerMovement movement = go.AddComponent<PlayerMovement>();
        movement.lastMoveDirection = Vector2.left;
        weapon.SetMovement(movement);

        float angle = weapon.CallGetSpawnAngle();

        Assert.AreEqual(100f, angle);
    }

    [Test]
    public void GetSpawnOffset_ShouldBeWithinVarianceRange()
    {
        GameObject go = new GameObject();
        TestAxeWeapon weapon = go.AddComponent<TestAxeWeapon>();

        weapon.SetCurrentStats(new Weapon.Stats
        {
            spawnVariance = new Rect(-1f, -2f, 2f, 4f)
        });

        Vector2 offset = weapon.CallGetSpawnOffset();

        Assert.IsTrue(offset.x >= -1f && offset.x <= 1f);
        Assert.IsTrue(offset.y >= -2f && offset.y <= 2f);
    }
}