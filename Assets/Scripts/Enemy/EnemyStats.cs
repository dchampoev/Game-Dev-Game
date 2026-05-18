using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyStats : MonoBehaviour
{
    [System.Serializable]
    public class Resitances
    {
        [Range(0f, 1f)] public float freeze = 0f, kill = 0f, debuff = 0f;

        public static Resitances operator *(Resitances r, float multiplier)
        {
            r.freeze = Mathf.Min(1, r.freeze * multiplier);
            r.kill = Mathf.Min(1, r.kill * multiplier);
            r.debuff = Mathf.Min(1, r.debuff * multiplier);
            return r;
        }
    }

    [System.Serializable]
    public struct Stats
    {
        [Min(0)] public float maxHealth, moveSpeed, damage;
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
            if ((boostable & Boostable.resistances) != 0) s1.resistances *= factor;
            return s1;
        }

        public static Stats operator *(Stats s1, float factor) { return Boost(s1, factor, s1.curseBoosts); }

        public static Stats operator ^(Stats s1, float factor) { return Boost(s1, factor, s1.levelBoosts); }
    }

    public Stats baseStats = new Stats { maxHealth = 10, moveSpeed = 1, damage = 3, knockbackMultiplier = 1 };
    Stats actualStats;
    public Stats Actual
    {
        get { return actualStats; }
    }

    float currentHealth;

    Transform player;

    [Header("Damage Feedback")]
    public Color damageColor = new Color(1, 0, 0, 1);
    public float damageFlashDuration = 0.2f;
    public float deathFadeDuration = 0.6f;
    Color originalColor;
    SpriteRenderer spriteRenderer;
    EnemyMovement enemyMovement;

    public static int count;
    void Awake()
    {
        count++;
    }

    void Start()
    {
        RecalculateStats();
        currentHealth = actualStats.maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        enemyMovement = GetComponent<EnemyMovement>();
    }
    
    void RecalculateStats()
    {
        float curse = GameManager.GetCumulativeCurse(),
              level = GameManager.GetCumulativeLevels();
        actualStats = (baseStats * curse) ^ level;
    }

    public void TakeDamage(float damage, Vector2 sourcePosition, float knockbackForce = 5f, float knockbackDuration = 0.2f)
    {
        if (Mathf.Approximately(damage, actualStats.maxHealth))
        {
            if(Random.value < actualStats.resistances.kill) return;
        }

        currentHealth -= damage;
        StartCoroutine(DamageFlash());

        if (damage > 0) GameManager.GenerateFloatingText(Mathf.FloorToInt(damage).ToString(), transform);

        if (knockbackForce > 0)
        {
            Vector2 knockbackDirection = ((Vector2)transform.position - sourcePosition).normalized;
            enemyMovement.Knockback(knockbackDirection * knockbackForce, knockbackDuration);
        }

        if (currentHealth <= 0)
        {
            Kill();
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent(out PlayerStats player))
        {
            player.TakeDamage(Actual.damage);
        }
    }

    private void OnDestroy()
    {
        count--;     
    }
    
    private void Kill()
    {
        DropRateManager drops = GetComponent<DropRateManager>();
        if(drops) drops.active = true;

        StartCoroutine(KillFade());
    }
    IEnumerator DamageFlash()
    {
        spriteRenderer.color = damageColor;
        yield return new WaitForSeconds(damageFlashDuration);
        spriteRenderer.color = originalColor;
    }

    IEnumerator KillFade()
    {
        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        float time = 0, originalAlpha = spriteRenderer.color.a;

        while (time < deathFadeDuration)
        {
            yield return wait;
            time += Time.deltaTime;

            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, (1 - time / deathFadeDuration) * originalAlpha);
        }
        
        Destroy(gameObject);
    }
}
