using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stats is a class that is inhereted by both PlayerStats and EnemyStats.
/// It is here to provide a way for Buffs to be applied to both PlayersStats 
/// and EnemyStats.
/// </summary>

public abstract class EntityStats : MonoBehaviour
{
    protected float health;

    protected SpriteRenderer sprite;
    protected Animator animator;
    protected Color originialColor;
    protected List<Color> appliedTints = new List<Color>();
    public const float TINT_FACTOR = 4f;

    [System.Serializable]
    public class Buff
    {
        public BuffData data;
        public float remainingDuration, nextTick;
        public int variant;

        public ParticleSystem effect;
        public Color tint;
        public float animationSpeed = 1f;

        public Buff(BuffData d, EntityStats owner, int variant = 0, float durationMultiplier = 1f)
        {
            data = d;
            BuffData.Stats buffStats = d.Get(variant);
            remainingDuration = buffStats.duration * durationMultiplier;
            nextTick = buffStats.tickInterval;
            this.variant = variant;

            if (buffStats.effect) effect = Instantiate(buffStats.effect, owner.transform);
            if (buffStats.tint.a > 0)
            {
                tint = buffStats.tint;
                owner.ApplyTint(buffStats.tint);
            }

            animationSpeed = buffStats.animationSpeed;
            owner.ApplyAnimationMultiplier(animationSpeed);
        }

        public BuffData.Stats GetData()
        {
            return data.Get(variant);
        }
    }

    protected List<Buff> activeBuffs = new List<Buff>();

    [System.Serializable]
    public class BuffInfo
    {
        public BuffData data;
        public int variant;
        [Range(0f, 1f)] public float probability = 1f;
    }

    protected virtual void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        originialColor = sprite.color;
        animator = GetComponent<Animator>();
    }

    public virtual void ApplyAnimationMultiplier(float factor)
    {
        if (animator) animator.speed *= Mathf.Approximately(0,factor) ? 0.000001f : factor;
    }

    public virtual void RemoveAnimationMultiplier(float factor)
    {
        if (animator) animator.speed /= Mathf.Approximately(0, factor) ? 0.000001f : factor;
    }

    public virtual void ApplyTint(Color c)
    {
        appliedTints.Add(c);
        UpdateColor();
    }

    public virtual void RemoveTint(Color c)
    {
        appliedTints.Remove(c);
        UpdateColor();
    }

    protected virtual void UpdateColor()
    {
        Color targetedColor = originialColor;
        float totalWeight = 1f;
        foreach (Color c in appliedTints)
        {
            targetedColor = new Color(
                targetedColor.r + c.r * c.a * TINT_FACTOR,
                targetedColor.g + c.g * c.a * TINT_FACTOR,
                targetedColor.b + c.b * c.a * TINT_FACTOR,
                targetedColor.a
            );
            totalWeight += c.a * TINT_FACTOR;
        }
        targetedColor = new Color(
            targetedColor.r / totalWeight,
            targetedColor.g / totalWeight,
            targetedColor.b / totalWeight,
            targetedColor.a
        );

        sprite.color = targetedColor;
    }

    public virtual Buff GetBuff(BuffData data, int variant = -1)
    {
        foreach (Buff buff in activeBuffs)
        {
            if (buff.data == data)
            {
                if (variant >= 0)
                {
                    if (buff.variant == variant) return buff;
                }
                else
                {
                    return buff;
                }
            }
        }
        return null;
    }

    public virtual bool ApplyBuff(BuffInfo info, float durationMultiplier = 1f)
    {
        if (Random.value <= info.probability)
        {
            return ApplyBuff(info.data, info.variant, durationMultiplier);
        }
        return false;
    }

    public virtual bool ApplyBuff(BuffData data, int variant = 0, float durationMultiplier = 1f)
    {
        Buff buff;
        BuffData.Stats stats = data.Get(variant);

        switch (stats.stackType)
        {
            case BuffData.StackType.stacksFully:
                activeBuffs.Add(new Buff(data, this, variant, durationMultiplier));
                RecalculateStats();
                return true;

            case BuffData.StackType.refreshDurationOnly:
                buff = GetBuff(data, variant);
                if (buff != null)
                {
                    buff.remainingDuration = stats.duration * durationMultiplier;
                }
                else
                {
                    activeBuffs.Add(new Buff(data, this, variant, durationMultiplier));
                    RecalculateStats();
                }
                return true;

            case BuffData.StackType.doesNotStack:
                buff = GetBuff(data, variant);
                if (buff == null)
                {
                    activeBuffs.Add(new Buff(data, this, variant, durationMultiplier));
                    RecalculateStats();
                    return true;
                }
                return false;
        }

        return false;
    }

    public virtual bool RemoveBuff(BuffData data, int variant = -1)
    {
        List<Buff> buffsToRemove = new List<Buff>();
        foreach (Buff buff in activeBuffs)
        {
            if (buff.data == data)
            {
                if (variant >= 0)
                {
                    if (buff.variant == variant) buffsToRemove.Add(buff);
                }
                else
                {
                    buffsToRemove.Add(buff);
                }
            }
        }

        if (buffsToRemove.Count > 0)
        {
            foreach (Buff buff in buffsToRemove)
            {
                if (buff.effect) Destroy(buff.effect.gameObject);
                if (buff.tint.a > 0) RemoveTint(buff.tint);
                RemoveAnimationMultiplier(buff.animationSpeed);
                activeBuffs.Remove(buff);
            }
            RecalculateStats();
            return true;
        }
        return false;
    }

    public abstract void TakeDamage(float damage);

    public abstract void RestoreHealth(float amount);

    public abstract void Kill();

    public abstract void RecalculateStats();

    protected virtual void Update()
    {
        List<Buff> expired = new List<Buff>();
        foreach (Buff buff in activeBuffs)
        {
            BuffData.Stats stats = buff.data.Get(buff.variant);

            buff.nextTick -= Time.deltaTime;
            if (buff.nextTick < 0)
            {
                float tickDamage = buff.data.GetTickDamage(buff.variant);
                if (tickDamage > 0) TakeDamage(tickDamage);
                float tickHeal = buff.data.GetTickHeal(buff.variant);
                if (tickHeal > 0) RestoreHealth(tickHeal);
                buff.nextTick = stats.tickInterval;
            }

            if (stats.duration <= 0) continue;

            buff.remainingDuration -= Time.deltaTime;
            if (buff.remainingDuration < 0) expired.Add(buff);
        }

        foreach (Buff buff in expired)
        {
            if (buff.effect) Destroy(buff.effect.gameObject);
            if (buff.tint.a > 0) RemoveTint(buff.tint);
            RemoveAnimationMultiplier(buff.animationSpeed);
            activeBuffs.Remove(buff);
        }
        RecalculateStats();
    }
}
