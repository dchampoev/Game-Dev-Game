using UnityEngine;

/// <summary>
/// Component that is attached to all Projectile prefabs. All spawned projectiles will fly
/// in the direction they are spawned in and deal damage when they hit an object.
/// </summary>
public class Projectile : WeaponEffect
{
    public enum DamageSource { projectile, owner };
    public DamageSource damageSource = DamageSource.projectile;
    public bool hasAutoAim = false;
    public Vector3 rotationSpeed = new Vector3(0, 0, 0);

    protected Rigidbody2D rigidBody;
    protected int piercing;

    protected virtual void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        Weapon.Stats stats = weapon.GetStats();
        if (rigidBody.bodyType == RigidbodyType2D.Dynamic)
        {
            rigidBody.angularVelocity = rotationSpeed.z;
            rigidBody.linearVelocity = transform.right * stats.speed * weapon.Owner.Stats.speed;
        }

        float area = weapon.GetArea();
        if(area <= 0) area = 1;
        transform.localScale = new Vector3(
            area * Mathf.Sign(transform.localScale.x),
            area * Mathf.Sign(transform.localScale.y),
            1
        );

        piercing = stats.piercing;

        if (stats.lifespan > 0) Destroy(gameObject, stats.lifespan);

        if (hasAutoAim) AcquireAutoAimFacing();
    }
    public virtual void AcquireAutoAimFacing()
    {
        float aimAngle;

        EnemyStats[] targets = FindObjectsByType<EnemyStats>(FindObjectsSortMode.None);

        if (targets.Length > 0)
        {
            EnemyStats selectedTarget = targets[Random.Range(0, targets.Length)];
            Vector2 differnce = selectedTarget.transform.position - transform.position;
            aimAngle = Mathf.Atan2(differnce.y, differnce.x) * Mathf.Rad2Deg;
        }
        else
        {
            aimAngle = Random.Range(0f, 360f);
        }

        transform.rotation = Quaternion.Euler(0, 0, aimAngle);
    }

    protected virtual void FixedUpdate()
    {
        if (rigidBody.bodyType == RigidbodyType2D.Kinematic)
        {
            Weapon.Stats stats = weapon.GetStats();
            transform.position += transform.right * stats.speed* weapon.Owner.Stats.speed * Time.fixedDeltaTime;
            rigidBody.MovePosition(transform.position);
            transform.Rotate(rotationSpeed * Time.fixedDeltaTime);
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collison)
    {
        EnemyStats enemies = collison.GetComponent<EnemyStats>();
        BreakableProps props = collison.GetComponent<BreakableProps>();

        if (enemies)
        {
            Vector3 source = damageSource == DamageSource.owner && owner ? owner.transform.position : transform.position;

            enemies.TakeDamage(GetDamage(), source);

            Weapon.Stats stats = weapon.GetStats();
            piercing--;
            if (stats.hitEffect)
            {
                Destroy(Instantiate(stats.hitEffect, transform.position, Quaternion.identity), 5f);
            }
        }
        else if (props)
        {
            props.TakeDamage(GetDamage());
            piercing--;

            Weapon.Stats stats = weapon.GetStats();
            if (stats.hitEffect)
            {
                Destroy(Instantiate(stats.hitEffect, transform.position, Quaternion.identity), 5f);
            }
        }

        if (piercing <= 0) Destroy(gameObject);
    }
}