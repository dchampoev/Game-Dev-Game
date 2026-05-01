using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyStats : MonoBehaviour
{
    public float currentMoveSpeed;
    public float currentHealth;
    public float currentDamage;

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
        PlayerStats foundPlayer = FindAnyObjectByType<PlayerStats>();
        if (foundPlayer == null) return;
        player = foundPlayer.transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        enemyMovement = GetComponent<EnemyMovement>();
    }


    public void TakeDamage(float damage, Vector2 sourcePosition, float knockbackForce = 5f, float knockbackDuration = 0.2f)
    {
        currentHealth -= damage;
        StartCoroutine(DamageFlash());

        if(damage > 0) GameManager.GenerateFloatingText(Mathf.FloorToInt(damage).ToString(), transform);

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
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerStats player = collision.gameObject.GetComponent<PlayerStats>();
            if (player != null)
            {
                player.TakeDamage(currentDamage);
            }
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
