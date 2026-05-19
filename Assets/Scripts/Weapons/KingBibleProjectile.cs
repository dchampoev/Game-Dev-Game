using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KingBibleProjectile : Projectile
{
    Dictionary<EnemyStats, float> affectedTargets = new Dictionary<EnemyStats, float>();
    List<EnemyStats> targetsToUnaffect = new List<EnemyStats>();
    public float hitDelay = 1.7f;

    public float speedMultiplier = 5f; 

    public float radiusMultiplier = 1.1f; 
    public float projectileSizeMultiplier = 0.9f;

    public float transitionTime = 0.5f;

    private float currentLifespan;

    Vector3 startScale;
    float startLifespan;
    bool isAlive = false;
    private float angle;

    protected override void Start()
    {
        base.Start();
        startScale = new Vector3(area, area,1);
        startLifespan = weapon.GetLifespan();
        transform.localScale = Vector3.zero;
        StartCoroutine(BibleGrow());
        Vector3 offset = transform.position - owner.transform.position; //Get the initial spawn position
        angle = Mathf.Atan2(offset.y, offset.x); // Get the angle in radians
    }

    protected override void FixedUpdate()
    {
        if (rigidBody && rigidBody.bodyType == RigidbodyType2D.Kinematic)
        {
            float x = owner.transform.position.x + Mathf.Cos(angle) * startScale.x * radiusMultiplier;
            float y = owner.transform.position.y + Mathf.Sin(angle) * startScale.y * radiusMultiplier;
            rigidBody.MovePosition(new Vector3(x, y, 0));
            angle -= weapon.GetStats().speed * speedMultiplier * Time.fixedDeltaTime;
        }
    }
    private void Update()
    {
        HitDelay();

        currentLifespan += Time.deltaTime;

        if (!rigidBody)
        {
            float x = owner.transform.position.x + Mathf.Cos(angle) * startScale.x * radiusMultiplier;
            float y = owner.transform.position.y + Mathf.Sin(angle) * startScale.y * radiusMultiplier;
            transform.position = new Vector3(x, y, 0);
            angle -= weapon.GetStats().speed * speedMultiplier * Time.deltaTime;
        }

        if (currentLifespan > startLifespan-transitionTime && isAlive)
        {
            StartCoroutine(BibleShrink());
            isAlive = false;
        }
        if (!weapon && isAlive)
        {
            StartCoroutine(BibleShrink());
            isAlive = false;
        }
    }
    public void HitDelay()
    {
        Dictionary<EnemyStats, float> affectedTargsCopy = new Dictionary<EnemyStats, float>(affectedTargets);

        foreach (KeyValuePair<EnemyStats, float> pair in affectedTargsCopy)
        {
            if (pair.Key)
            {
                Vector3 source = damageSource == DamageSource.owner && owner ? owner.transform.position : transform.position;
                affectedTargets[pair.Key] -= Time.deltaTime;
                if (pair.Value <= 0)
                {
                    if (targetsToUnaffect.Contains(pair.Key))
                    {
                        affectedTargets.Remove(pair.Key);
                        targetsToUnaffect.Remove(pair.Key);
                    }
                    else
                    {
                        Weapon.Stats stats = weapon.GetStats();
                        affectedTargets[pair.Key] = hitDelay;
                        pair.Key.TakeDamage(GetDamage(), source, stats.knockback);

                        weapon.ApplyBuffs(pair.Key); 

                        if (stats.hitEffect)
                        {
                            Destroy(Instantiate(stats.hitEffect, pair.Key.transform.position, Quaternion.identity).gameObject, 5f);
                        }
                        piercing--;
                    }
                }
            }

        }
    }

    public IEnumerator BibleShrink()
    {
        Vector3 currentScale = transform.localScale;

        WaitForEndOfFrame w = new WaitForEndOfFrame();
        float t = 0;

        while (t < transitionTime)
        {
            yield return w;
            t += Time.deltaTime;

           transform.localScale = new Vector3(currentScale.x - (t/ transitionTime), currentScale.y - (t / transitionTime), 1f);
        }
        if (!weapon) Destroy(gameObject);
    }
    public IEnumerator BibleGrow()
    {
        if (!isAlive)
        {
            isAlive = true;
            WaitForEndOfFrame w = new WaitForEndOfFrame();
            float t = 0;

            while (t < transitionTime)
            {
                yield return w;
                t += Time.deltaTime;

                transform.localScale = new Vector3(0 + (t/ transitionTime * startScale.x) * projectileSizeMultiplier, 0 + (t/ transitionTime * startScale.y) * projectileSizeMultiplier, 1f);
            }

        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out EnemyStats es))
        {
            if (!affectedTargets.ContainsKey(es))
            {
                affectedTargets.Add(es, 0);
            }
        }
        else if (other.TryGetComponent(out BreakableProps p))
        {
            p.TakeDamage(GetDamage());
            piercing--;

            Weapon.Stats stats = weapon.GetStats();
            if (stats.hitEffect)
            {
                Destroy(Instantiate(stats.hitEffect, transform.position, Quaternion.identity).gameObject, 5f);
            }
        }
        if (piercing <= 0) Destroy(gameObject);
    }
}