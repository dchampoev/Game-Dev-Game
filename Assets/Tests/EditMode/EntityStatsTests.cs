using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class EntityStatsTests
{
    private class TestEntityStats : EntityStats
    {
        public float damageTaken;
        public float healthRestored;
        public int recalculationCount;
        public int killCount;

        public int ActiveBuffCount => activeBuffs.Count;

        public Buff FirstBuff => activeBuffs.Count > 0 ? activeBuffs[0] : null;

        public void CallStart()
        {
            typeof(EntityStats)
                .GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(this, null);
        }

        public void CallUpdate()
        {
            typeof(EntityStats)
                .GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(this, null);
        }

        public override void TakeDamage(float damage)
        {
            damageTaken += damage;
        }

        public override void RestoreHealth(float amount)
        {
            healthRestored += amount;
        }

        public override void Kill()
        {
            killCount++;
        }

        public override void RecalculateStats()
        {
            recalculationCount++;
        }
    }

    private BuffData CreateBuff(
        BuffData.StackType stackType = BuffData.StackType.refreshDurationOnly,
        float duration = 10f,
        float tickInterval = 0.25f,
        float damagePerSecond = 0f,
        float healPerSecond = 0f,
        Color? tint = null,
        float animationSpeed = 1f)
    {
        BuffData data = ScriptableObject.CreateInstance<BuffData>();
        data.variations = new[]
        {
            new BuffData.Stats
            {
                duration = duration,
                tickInterval = tickInterval,
                damagePerSecond = damagePerSecond,
                healPerSecond = healPerSecond,
                stackType = stackType,
                tint = tint ?? new Color(0, 0, 0, 0),
                animationSpeed = animationSpeed
            }
        };
        return data;
    }

    private TestEntityStats CreateEntity()
    {
        GameObject entityObject = new GameObject("Entity");
        SpriteRenderer renderer = entityObject.AddComponent<SpriteRenderer>();
        renderer.color = Color.white;
        TestEntityStats entity = entityObject.AddComponent<TestEntityStats>();
        entity.CallStart();
        return entity;
    }

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(obj);
        }

        TestScriptableObjectCleanup.DestroyRuntimeObjects<BuffData>();
    }

    [Test]
    public void ApplyBuff_WhenStackTypeStacksFully_ShouldAddMultipleBuffInstances()
    {
        TestEntityStats entity = CreateEntity();
        BuffData buff = CreateBuff(BuffData.StackType.stacksFully);

        bool firstResult = entity.ApplyBuff(buff);
        bool secondResult = entity.ApplyBuff(buff);

        Assert.IsTrue(firstResult);
        Assert.IsTrue(secondResult);
        Assert.AreEqual(2, entity.ActiveBuffCount);
    }

    [Test]
    public void ApplyBuff_WhenRefreshDurationOnly_ShouldRefreshExistingBuffWithoutAddingDuplicate()
    {
        TestEntityStats entity = CreateEntity();
        BuffData buff = CreateBuff(BuffData.StackType.refreshDurationOnly, duration: 5f);

        entity.ApplyBuff(buff);
        entity.FirstBuff.remainingDuration = 1f;

        bool result = entity.ApplyBuff(buff, durationMultiplier: 2f);

        Assert.IsTrue(result);
        Assert.AreEqual(1, entity.ActiveBuffCount);
        Assert.AreEqual(10f, entity.FirstBuff.remainingDuration);
    }

    [Test]
    public void ApplyBuff_WhenDoesNotStack_ShouldApplyOnceAndRejectDuplicate()
    {
        TestEntityStats entity = CreateEntity();
        BuffData buff = CreateBuff(BuffData.StackType.doesNotStack);

        bool firstResult = entity.ApplyBuff(buff);
        bool secondResult = entity.ApplyBuff(buff);

        Assert.IsTrue(firstResult);
        Assert.IsFalse(secondResult);
        Assert.AreEqual(1, entity.ActiveBuffCount);
    }

    [Test]
    public void ApplyBuffInfo_WhenProbabilityIsZero_ShouldNotApplyBuff()
    {
        TestEntityStats entity = CreateEntity();
        BuffData buff = CreateBuff();

        bool result = entity.ApplyBuff(new EntityStats.BuffInfo
        {
            data = buff,
            probability = 0f
        });

        Assert.IsFalse(result);
        Assert.AreEqual(0, entity.ActiveBuffCount);
    }

    [Test]
    public void RemoveBuff_ShouldRemoveMatchingBuffAndRecalculateStats()
    {
        TestEntityStats entity = CreateEntity();
        BuffData buff = CreateBuff();
        entity.ApplyBuff(buff);
        int recalculationsAfterApply = entity.recalculationCount;

        bool result = entity.RemoveBuff(buff);

        Assert.IsTrue(result);
        Assert.AreEqual(0, entity.ActiveBuffCount);
        Assert.Greater(entity.recalculationCount, recalculationsAfterApply);
    }

    [Test]
    public void Update_WhenBuffTickIsDue_ShouldApplyTickDamageAndHealing()
    {
        TestEntityStats entity = CreateEntity();
        BuffData buff = CreateBuff(
            duration: 10f,
            tickInterval: 0.5f,
            damagePerSecond: 4f,
            healPerSecond: 2f);

        entity.ApplyBuff(buff);
        entity.FirstBuff.nextTick = -0.01f;

        entity.CallUpdate();

        Assert.AreEqual(2f, entity.damageTaken);
        Assert.AreEqual(1f, entity.healthRestored);
        Assert.AreEqual(0.5f, entity.FirstBuff.nextTick);
    }

    [Test]
    public void Update_WhenBuffExpired_ShouldRemoveBuff()
    {
        TestEntityStats entity = CreateEntity();
        BuffData buff = CreateBuff(duration: 1f);
        entity.ApplyBuff(buff);
        entity.FirstBuff.remainingDuration = -0.01f;

        entity.CallUpdate();

        Assert.AreEqual(0, entity.ActiveBuffCount);
    }
}
