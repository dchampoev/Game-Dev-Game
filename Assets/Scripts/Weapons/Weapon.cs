using UnityEngine;

/// <summary>
/// Component to be attached to all Weapon prefabs. The Weapon prefab works together with WeaponData 
/// ScriptableObject to manage and run the behaviours of all weapons in the game.
/// </summary>

public abstract class Weapon : Item
{
    [System.Serializable]
    public struct Stats
    {
        public string name, description;

        [Header("Visuals")]
        public Projectile projectilePrefab;
        public Aura auraPrefab;
        public ParticleSystem hitEffect, procEffect;
        public Rect spawnVariance;

        [Header("Values")]
        public float lifespan;
        public float damage, damageVariance, area, speed, cooldown, projectileInterval, knockback;
        public int number, piercing, maxInstances;

        public static Stats operator +(Stats s1, Stats s2)
        {
            Stats result = new Stats();

            result.name = string.IsNullOrEmpty(s2.name) ? s1.name : s2.name;
            result.description = string.IsNullOrEmpty(s2.description) ? s1.description : s2.description;

            result.projectilePrefab = s2.projectilePrefab != null ? s2.projectilePrefab : s1.projectilePrefab;
            result.auraPrefab = s2.auraPrefab != null ? s2.auraPrefab : s1.auraPrefab;
            result.hitEffect = s2.hitEffect != null ? s2.hitEffect : s1.hitEffect;
            result.procEffect = s2.procEffect != null ? s2.procEffect : s1.procEffect;

            result.spawnVariance = s2.spawnVariance;

            result.lifespan = s1.lifespan + s2.lifespan;
            result.damage = s1.damage + s2.damage;
            result.damageVariance = s1.damageVariance + s2.damageVariance;
            result.area = s1.area + s2.area;
            result.speed = s1.speed + s2.speed;
            result.cooldown = s1.cooldown + s2.cooldown;
            result.projectileInterval = s1.projectileInterval + s2.projectileInterval;
            result.knockback = s1.knockback + s2.knockback;

            result.number = s1.number + s2.number;
            result.piercing = s1.piercing + s2.piercing;
            result.maxInstances = s1.maxInstances + s2.maxInstances;

            return result;
        }

        public float GetDamage()
        {
            return damage + Random.Range(0, damageVariance);
        }
    }
    protected Stats currentStats;
    public WeaponData data;
    protected float currentCooldown;
    protected PlayerMovement movement;

    public virtual void Initialize(WeaponData data)
    {
        base.Initialize(data);

        this.data = data;
        currentStats = data.baseStats;
        movement = owner.GetComponent<PlayerMovement>();
        ActivateCooldown();
    }
    protected virtual void Update()
    {
        currentCooldown -= Time.deltaTime;
        if (currentCooldown <= 0f)
        {
            Attack(currentStats.number + owner.Stats.amount);
        }
    }

    public override bool DoLevelUp()
    {
        base.DoLevelUp();
        if (!CanLevelUp())
        {
            Debug.LogWarning(string.Format("Cannot level up {0} to Level {1}, max level of {2} already reached.", name, currentLevel, data.maxLevel));
            return false;
        }

        currentStats += data.GetLevelData(++currentLevel);
        return true;
    }

    public virtual bool CanAttack()
    {
        return currentCooldown <= 0f;
    }

    protected virtual bool Attack(int attackCount = 1)
    {
        if (CanAttack())
        {
            currentCooldown += currentStats.cooldown;
            return true;
        }
        return false;
    }

    public virtual float GetDamage()
    {
        return currentStats.GetDamage() * owner.Stats.might;
    }

    public virtual float GetArea()
    {
        return currentStats.area + owner.Stats.area;
    }

    public virtual Stats GetStats()
    {
        return currentStats;
    }

    public virtual bool ActivateCooldown(bool strict = false)
    {
        if (strict && currentCooldown > 0) return false;

        float actualCooldown = currentStats.cooldown * Owner.Stats.cooldown;
        currentCooldown = Mathf.Min(actualCooldown, currentCooldown + actualCooldown);
        return true;
    }
}