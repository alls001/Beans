using UnityEngine;
using System.Collections;

public class EnemyMelee : EnemyBase
{
    [Header("Melee")]
    public Transform attackPoint;
    public float meleeRange = 1.2f;
    public float attackStartDistance = 1.6f;
    public float meleeDamage = 2f;
    public float meleeCooldown = 1.2f;
    public float meleeWindup = 0.2f;
    public string meleeAnimationName = "Near Attack";

    private float nextMeleeTime = 0f;
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

        // Quando entra nessa distância, ele para de perseguir e começa a atacar
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

        Vector3 toPlayer = playerTransform.position - transform.position;
        toPlayer.y = 0f;
        SetMovement(toPlayer.normalized, moveSpeed);
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
    }
}