using UnityEngine;
using System.Collections;

public class EnemyMeleeFireball : EnemyBase
{
    [Header("Melee")]
    public Transform attackPoint;
    public float meleeRange = 1.2f;
    public float attackStartDistance = 1.8f;
    public float meleeDamage = 2f;
    public float meleeCooldown = 1.2f;
    public float meleeWindup = 0.2f;
    public string meleeAnimationName = "Near Attack";

    [Header("Fireball")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float rangedMinDistance = 2.5f;
    public float rangedMaxDistance = 10f;
    public float fireballCooldown = 2f;
    public float fireballWindup = 0.25f;
    public float projectileSpeed = 10f;
    public bool keepMovingWhilePreparingFireball = true;
    public string rangedAnimationName = "Ranger-Attack";

    private float nextMeleeTime = 0f;
    private float nextFireballTime = 0f;
    private Coroutine attackRoutine;

    protected override void UpdateBehavior()
    {
        if (playerTransform == null)
        {
            StopMovement();
            return;
        }

        if (isAttacking) return;

        float distance = DistanceToPlayerXZ();

        if (distance > detectionDistance)
        {
            StopMovement();
            return;
        }

        // PARA DE PERSEGUIR ANTES DE ENTRAR EM CIMA DO PLAYER
        if (distance <= attackStartDistance)
        {
            StopMovement();

            if (Time.time >= nextMeleeTime)
            {
                if (attackRoutine != null) StopCoroutine(attackRoutine);
                attackRoutine = StartCoroutine(MeleeAttackRoutine());
                nextMeleeTime = Time.time + meleeCooldown;
            }

            return;
        }

        bool canShoot =
            projectilePrefab != null &&
            firePoint != null &&
            distance >= rangedMinDistance &&
            distance <= rangedMaxDistance &&
            Time.time >= nextFireballTime;

        if (canShoot)
        {
            if (attackRoutine != null) StopCoroutine(attackRoutine);
            attackRoutine = StartCoroutine(FireballRoutine());
            nextFireballTime = Time.time + fireballCooldown;

            if (keepMovingWhilePreparingFireball)
            {
                Vector3 toPlayer = playerTransform.position - transform.position;
                toPlayer.y = 0f;
                SetMovement(toPlayer.normalized, moveSpeed);
            }
            else
            {
                StopMovement();
            }

            return;
        }

        Vector3 chaseDir = playerTransform.position - transform.position;
        chaseDir.y = 0f;
        SetMovement(chaseDir.normalized, moveSpeed);
    }

    IEnumerator MeleeAttackRoutine()
    {
        isAttacking = true;
        StopMovement();

        ForcePlayQuadAnimation(meleeAnimationName);

        yield return new WaitForSeconds(meleeWindup);

        if (!isDead && !isTakingHit)
            PerformMeleeDamage();

        yield return new WaitForSeconds(0.08f);

        isAttacking = false;
        attackRoutine = null;
    }

    IEnumerator FireballRoutine()
    {
        isAttacking = true;

        ForcePlayQuadAnimation(rangedAnimationName);

        yield return new WaitForSeconds(fireballWindup);

        if (!isDead && !isTakingHit)
            FireProjectile();

        yield return new WaitForSeconds(0.08f);

        isAttacking = false;
        attackRoutine = null;
    }

    void PerformMeleeDamage()
    {
        if (attackPoint == null) return;

        Collider[] hitPlayers = Physics.OverlapSphere(attackPoint.position, meleeRange, playerLayer);

        foreach (Collider c in hitPlayers)
        {
            HealthSystem hs = c.GetComponent<HealthSystem>();
            if (hs != null)
                hs.TakeDamage(meleeDamage, transform);
        }
    }

    void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null || playerTransform == null)
            return;

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        Vector3 dir = playerTransform.position - firePoint.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
            dir = transform.forward;

        dir.Normalize();

        projectile.transform.forward = dir;

        Rigidbody projRb = projectile.GetComponent<Rigidbody>();
        if (projRb != null)
        {
            projRb.isKinematic = false;
            projRb.linearVelocity = dir * projectileSpeed;
        }

        Destroy(projectile, 5f);
    }

    public override void OnTakeDamage()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        base.OnTakeDamage();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackStartDistance);

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, meleeRange);
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, rangedMinDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, rangedMaxDistance);
    }
}