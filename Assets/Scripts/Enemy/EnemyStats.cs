using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyStats : EntityStats
{
    [System.Serializable]
    public class Resitances
    {
        [Range(-1f, 1f)] public float freeze = 0f, kill = 0f, debuff = 0f;

        public static Resitances operator *(Resitances r, float multiplier)
        {
            if (r == null) return new Resitances();
            r.freeze = Mathf.Min(1, r.freeze * multiplier);
            r.kill = Mathf.Min(1, r.kill * multiplier);
            r.debuff = Mathf.Min(1, r.debuff * multiplier);
            return r;
        }

        public static Resitances operator +(Resitances r, Resitances r2)
        {
            if (r == null) r = new Resitances();
            if (r2 == null) return r;
            r.freeze += r2.freeze;
            r.kill = r2.kill;
            r.debuff = r2.debuff;
            return r;
        }

        public static Resitances operator *(Resitances r1, Resitances r2)
        {
            if (r1 == null) r1 = new Resitances { freeze = 1f, kill = 1f, debuff = 1f };
            if (r2 == null) return r1;
            r1.freeze = Mathf.Min(1, r1.freeze * r2.freeze);
            r1.kill = Mathf.Min(1, r1.kill * r2.kill);
            r1.debuff = Mathf.Min(1, r1.debuff * r2.debuff);
            return r1;
        }
    }

    [System.Serializable]
    public struct Stats
    {
        public float maxHealth, moveSpeed, damage;
        public float knockbackMultiplier;
        public Resitances resistances;

        [System.Flags]
        public enum Boostable { health = 1, moveSpeed = 2, damage = 4, knockback = 8, resistances = 16 }
        public Boostable curseBoosts, levelBoosts;

        private static Stats Boost(Stats s1, float factor, Boostable boostable)
        {
            if ((boostable & Boostable.health) != 0) s1.maxHealth *= factor;
            if ((boostable & Boostable.moveSpeed) != 0) s1.moveSpeed *= factor;
            if ((boostable & Boostable.damage) != 0) s1.damage *= factor;
            if ((boostable & Boostable.knockback) != 0) s1.knockbackMultiplier /= factor;
            if ((boostable & Boostable.resistances) != 0 && s1.resistances != null) s1.resistances *= factor;
            return s1;
        }

        public static Stats operator *(Stats s1, float factor) { return Boost(s1, factor, s1.curseBoosts); }
        public static Stats operator ^(Stats s1, float factor) { return Boost(s1, factor, s1.levelBoosts); }

        public static Stats operator +(Stats s1, Stats s2)
        {
            s1.maxHealth += s2.maxHealth;
            s1.moveSpeed += s2.moveSpeed;
            s1.damage += s2.damage;
            s1.knockbackMultiplier += s2.knockbackMultiplier;
            s1.resistances += s2.resistances;
            return s1;
        }

        public static Stats operator *(Stats s1, Stats s2)
        {
            s1.maxHealth *= s2.maxHealth;
            s1.moveSpeed *= s2.moveSpeed;
            s1.damage *= s2.damage;
            s1.knockbackMultiplier *= s2.knockbackMultiplier;
            s1.resistances *= s2.resistances;
            return s1;
        }
    }

    public Stats baseStats = new Stats { maxHealth = 10, moveSpeed = 1, damage = 3, knockbackMultiplier = 1 };
    Stats actualStats;

    public Stats Actual
    {
        get { return actualStats; }
    }

    public BuffInfo[] attackEffects;

    [Header("Damage Feedback")]
    public Color damageColor = new Color(1, 0, 0, 1);
    public float damageFlashDuration = 0.2f;
    public float deathFadeDuration = 0.6f;
    EnemyMovement enemyMovement;
    bool isDead;

    public static int count;

    void Awake()
    {
        count++;

        enemyMovement = GetComponent<EnemyMovement>();
    }

    protected override void Start()
    {
        base.Start();

        RecalculateStats();
        health = actualStats.maxHealth;
    }

    public override bool ApplyBuff(BuffData data, int variant = 0, float durationMultiplier = 1f)
    {
        if ((data.type & BuffData.Type.freeze) > 0)
        {
            if (Random.value <= Actual.resistances.freeze) return false;
        }

        if ((data.type & BuffData.Type.debuff) > 0)
        {
            if (Random.value <= Actual.resistances.debuff) return false;
        }
        
        return base.ApplyBuff(data, variant, durationMultiplier);
    }

    public override void RecalculateStats()
    {
        float curse = GameManager.GetCumulativeCurse();
        float level = GameManager.GetCumulativeLevels();

        actualStats = (baseStats * curse) ^ level;

        Stats multiplier = new Stats
        {
            maxHealth = 1f,
            moveSpeed = 1f,
            damage = 1f,
            knockbackMultiplier = 1,
            resistances = new Resitances { freeze = 1f, kill = 1f, debuff = 1f }
        };

        foreach (Buff buff in activeBuffs)
        {
            BuffData.Stats buffStats = buff.GetData();
            switch (buffStats.modifierType)
            {
                case BuffData.ModifierType.additive:
                    actualStats += buffStats.enemyModifier;
                    break;
                case BuffData.ModifierType.multiplicative:
                    multiplier *= buffStats.enemyModifier;
                    break;
            }
        }

        actualStats *= multiplier;

        if (actualStats.resistances == null)
            actualStats.resistances = new Resitances();
    }

    public override void TakeDamage(float damage)
    {
        if (damage == actualStats.maxHealth)
        {
            if (Random.value < actualStats.resistances.kill) return;
        }

        health -= damage;

        if (damage > 0)
        {
            StartCoroutine(DamageFlash());
            GameManager.GenerateFloatingText(Mathf.FloorToInt(damage).ToString(), transform);
        }

        if (health <= 0)
        {
            Kill();
        }
    }

    public void TakeDamage(float damage, Vector2 sourcePosition, float knockbackForce = 5f, float knockbackDuration = 0.2f)
    {
        if (isDead) return;

        TakeDamage(damage);

        if (enemyMovement != null && knockbackForce > 0)
        {
            Vector2 knockbackDirection = ((Vector2)transform.position - sourcePosition).normalized;
            enemyMovement.Knockback(knockbackDirection * knockbackForce, knockbackDuration);
        }
    }

    public override void RestoreHealth(float amount)
    {
        if (health < actualStats.maxHealth)
        {
            health += amount;
            if(health > actualStats.maxHealth) health = actualStats.maxHealth;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;

        if (Mathf.Approximately(Actual.damage, 0)) return;

        if (collision.collider.TryGetComponent(out PlayerStats player))
        {
            player.TakeDamage(Actual.damage);
            if (attackEffects == null) return;

            foreach(BuffInfo buff in attackEffects)
            {
                if (buff == null || buff.data == null) continue;
                player.ApplyBuff(buff);
            }
        }
    }

    void OnDestroy()
    {
        count = Mathf.Max(0, count - 1);
    }

    public override void Kill()
    {
        if (isDead) return;
        isDead = true;

        foreach (Collider2D collider in GetComponents<Collider2D>())
            collider.enabled = false;

        if (enemyMovement != null)
            enemyMovement.enabled = false;

        DropRateManager drops = GetComponent<DropRateManager>();
        if (drops) drops.active = true;

        StartCoroutine(KillFade());
    }

    IEnumerator DamageFlash()
    {
        if(sprite == null)
            yield break;
        
        sprite.color = damageColor;
        yield return new WaitForSeconds(damageFlashDuration);
        if(sprite != null && !isDead)
            UpdateColor();
    }

    IEnumerator KillFade()
    {
        if (sprite== null)
        {
            Destroy(gameObject);
            yield break;
        }

        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        float time = 0f;
        float originalAlpha = sprite.color.a;

        while (time < deathFadeDuration)
        {
            yield return wait;
            time += Time.deltaTime;

            if (sprite == null)
                yield break;

            Color currentColor = sprite.color;
            sprite.color = new Color(
                currentColor.r,
                currentColor.g,
                currentColor.b,
                (1f - time / deathFadeDuration) * originalAlpha
            );
        }

        Destroy(gameObject);
    }
}
