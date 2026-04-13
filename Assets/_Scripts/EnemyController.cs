using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class EnemyController : MonoBehaviour
{
    public enum BehaviorType { Stationary, Patrol, Ranged }

    [Header("Comportamento")]
    public BehaviorType enemyType = BehaviorType.Patrol;

    private enum AIState
    {
        Idle,
        Patrolling,
        Chasing,
        Attacking,
        RangedAttacking,
        Hit,
        Dead
    }

    private AIState currentState;

    [Header("Referências")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private QuadSpriteAnimator quadAnimator;

    [Header("Percepção")]
    public float detectionDistance = 10f;
    public bool autoDetectPlayer = true;

    [Header("Movimento")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 3f;
    public float stoppingDistance = 0.8f;
    public Transform[] patrolPoints;
    public float patrolWaitTime = 2f;
    public float patrolPointThreshold = 0.6f;

    [Header("Combate Melee")]
    public float attackDamage = 2f;
    public float attackRate = 1.5f;
    public Transform attackPoint;
    public float attackRange = 1f;
    public float meleeAttackWindup = 0.25f;

    [Header("Combate a Distância")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float rangedAttackDistance = 12f;
    public float rangedStoppingDistance = 8f;
    public float retreatDistance = 5f;
    public float projectileSpeed = 15f;
    public float rangedAttackRate = 2f;
    public float rangedAttackWindup = 0.35f;

    [Header("Detecção")]
    public LayerMask playerLayer;

    [Header("Hit / Death")]
    public float hitAnimationDuration = 0.35f;
    public float deathAnimationDuration = 1f;
    public float disappearDelayAfterDeath = 0.8f;

    [Header("Nomes das animações do Quad")]
    public string idleAnimationName = "Idle";
    public string walkAnimationName = "walk";
    public string meleeAttackAnimationName = "Near Attack";
    public string rangedAttackAnimationName = "Ranger-Attack";
    public string hitAnimationName = "Hit";
    public string dieAnimationName = "Die";

    private Rigidbody rb;
    private HealthSystem healthSystem;

    private Vector2 lookDirection = Vector2.right;
    private float nextAttackTime = 0f;
    private int currentPatrolIndex = 0;
    private bool isWaitingAtPatrolPoint = false;

    private Coroutine waitCoroutine = null;
    private Coroutine attackCoroutine = null;
    private Coroutine hitCoroutine = null;
    private Coroutine deathCoroutine = null;

    private bool isAttacking = false;
    private bool isTakingHit = false;
    private bool isDead = false;

    private float lastNonZeroHorizontal = 1f;
    private string currentQuadAnimation = "";

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        healthSystem = GetComponent<HealthSystem>();

        if (quadAnimator == null)
            quadAnimator = GetComponentInChildren<QuadSpriteAnimator>();

        if (playerTransform == null && autoDetectPlayer)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;
        }

        InitializeStartingState();
        ValidateSettings();

        ForcePlayQuadAnimation(idleAnimationName);
    }

    void InitializeStartingState()
    {
        switch (enemyType)
        {
            case BehaviorType.Stationary:
            case BehaviorType.Ranged:
                currentState = AIState.Idle;
                break;

            case BehaviorType.Patrol:
                currentState = (patrolPoints == null || patrolPoints.Length == 0)
                    ? AIState.Idle
                    : AIState.Patrolling;
                break;

            default:
                currentState = AIState.Idle;
                break;
        }
    }

    void ValidateSettings()
    {
        if (playerLayer == 0)
            Debug.LogWarning(gameObject.name + ": Player Layer não configurada.");

        if (enemyType != BehaviorType.Ranged && attackPoint == null)
            Debug.LogWarning(gameObject.name + ": attackPoint não configurado.");

        if (enemyType == BehaviorType.Ranged)
        {
            if (projectilePrefab == null)
                Debug.LogWarning(gameObject.name + ": projectilePrefab não configurado.");

            if (firePoint == null)
                Debug.LogWarning(gameObject.name + ": firePoint não configurado.");
        }
    }

    void Update()
    {
        if (isDead) return;

        if (playerTransform == null && autoDetectPlayer)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;
        }

        if (playerTransform != null)
        {
            Vector3 dir3D = playerTransform.position - transform.position;
            lookDirection = new Vector2(dir3D.x, dir3D.z).normalized;
        }

        UpdateTargetDetection();
        UpdateQuadAnimation();
        HandleFlip();
    }

    void FixedUpdate()
    {
        if (isDead)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        if (isTakingHit)
        {
            if (healthSystem != null && healthSystem.IsKnockedBack())
                return;

            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        if (!isWaitingAtPatrolPoint)
            ExecuteCurrentStateLogic();
    }

    void UpdateTargetDetection()
    {
        if (playerTransform == null) return;
        if (isDead || isTakingHit || isAttacking) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= detectionDistance)
        {
            if (currentState == AIState.Idle || currentState == AIState.Patrolling)
            {
                StopWaitingCoroutineIfNeeded();
                currentState = AIState.Chasing;
            }
        }
        else
        {
            if (currentState == AIState.Chasing || currentState == AIState.Attacking || currentState == AIState.RangedAttacking)
            {
                GoBackToDefaultState();
            }
        }
    }

    void ExecuteCurrentStateLogic()
    {
        switch (currentState)
        {
            case AIState.Idle:
                HandleIdleState();
                break;

            case AIState.Patrolling:
                HandlePatrolState();
                break;

            case AIState.Chasing:
                HandleChaseState();
                break;

            case AIState.Attacking:
                HandleAttackState();
                break;

            case AIState.RangedAttacking:
                HandleRangedAttackState();
                break;
        }
    }

    void HandleIdleState()
    {
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
    }

    void HandlePatrolState()
    {
        if (isAttacking) return;

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            currentState = AIState.Idle;
            return;
        }

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        float distance = Vector3.Distance(transform.position, targetPoint.position);

        if (distance < patrolPointThreshold)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);

            if (waitCoroutine == null)
                waitCoroutine = StartCoroutine(WaitAndMoveToNextPoint());
        }
        else
        {
            Vector3 dir3D = targetPoint.position - transform.position;
            lookDirection = new Vector2(dir3D.x, dir3D.z).normalized;

            Vector3 targetVelocity = new Vector3(lookDirection.x, 0f, lookDirection.y) * patrolSpeed;
            targetVelocity.y = rb.linearVelocity.y;
            rb.linearVelocity = targetVelocity;
        }
    }

    void HandleChaseState()
    {
        if (isAttacking) return;

        StopWaitingCoroutineIfNeeded();

        if (playerTransform == null)
        {
            GoBackToDefaultState();
            return;
        }

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (enemyType == BehaviorType.Ranged)
        {
            currentState = AIState.RangedAttacking;
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        if (distance <= Mathf.Max(stoppingDistance, attackRange))
        {
            currentState = AIState.Attacking;
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        Vector3 targetVelocity = new Vector3(lookDirection.x, 0f, lookDirection.y) * chaseSpeed;
        targetVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = targetVelocity;
    }

    void HandleAttackState()
    {
        StopWaitingCoroutineIfNeeded();

        if (playerTransform == null)
        {
            GoBackToDefaultState();
            return;
        }

        if (isAttacking)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance > attackRange + 0.5f)
        {
            currentState = AIState.Chasing;
            return;
        }

        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);

        if (Time.time >= nextAttackTime)
        {
            attackCoroutine = StartCoroutine(MeleeAttackRoutine());
            nextAttackTime = Time.time + attackRate;
        }
    }

    void HandleRangedAttackState()
    {
        StopWaitingCoroutineIfNeeded();

        if (playerTransform == null)
        {
            GoBackToDefaultState();
            return;
        }

        if (isAttacking)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance < retreatDistance)
        {
            Vector3 dirAway = (transform.position - playerTransform.position).normalized;
            Vector3 targetVelocity = new Vector3(dirAway.x, 0f, dirAway.y) * chaseSpeed;
            targetVelocity.y = rb.linearVelocity.y;
            rb.linearVelocity = targetVelocity;
            return;
        }

        if (distance > rangedAttackDistance)
        {
            currentState = AIState.Chasing;
            return;
        }

        if (distance > rangedStoppingDistance)
        {
            Vector3 dirToPlayer = (playerTransform.position - transform.position).normalized;
            lookDirection = new Vector2(dirToPlayer.x, dirToPlayer.z);

            Vector3 targetVelocity = new Vector3(lookDirection.x, 0f, lookDirection.y) * chaseSpeed;
            targetVelocity.y = rb.linearVelocity.y;
            rb.linearVelocity = targetVelocity;
        }
        else
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);

            if (Time.time >= nextAttackTime)
            {
                attackCoroutine = StartCoroutine(RangedAttackRoutine());
                nextAttackTime = Time.time + rangedAttackRate;
            }
        }
    }

    IEnumerator MeleeAttackRoutine()
    {
        isAttacking = true;
        currentState = AIState.Attacking;
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);

        ForcePlayQuadAnimation(meleeAttackAnimationName);

        yield return new WaitForSeconds(meleeAttackWindup);

        if (!isDead && playerTransform != null)
            PerformMeleeAttack();

        yield return new WaitForSeconds(0.1f);

        isAttacking = false;
        attackCoroutine = null;
    }

    IEnumerator RangedAttackRoutine()
    {
        isAttacking = true;
        currentState = AIState.RangedAttacking;
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);

        ForcePlayQuadAnimation(rangedAttackAnimationName);

        yield return new WaitForSeconds(rangedAttackWindup);

        if (!isDead && playerTransform != null)
            FireProjectile();

        yield return new WaitForSeconds(0.1f);

        isAttacking = false;
        attackCoroutine = null;
    }

    void PerformMeleeAttack()
    {
        if (attackPoint == null) return;

        Collider[] hitPlayers = Physics.OverlapSphere(attackPoint.position, attackRange, playerLayer);

        foreach (Collider playerCollider in hitPlayers)
        {
            HealthSystem playerHealth = playerCollider.GetComponent<HealthSystem>();
            if (playerHealth != null)
                playerHealth.TakeDamage(attackDamage, transform);
        }
    }

    void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null || playerTransform == null)
            return;

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        Vector3 dirToPlayer = playerTransform.position - firePoint.position;
        dirToPlayer.y = 0f;
        dirToPlayer.Normalize();

        Rigidbody projRb = projectile.GetComponent<Rigidbody>();
        if (projRb != null)
        {
            if (projRb.isKinematic) projRb.isKinematic = false;
            projRb.linearVelocity = dirToPlayer * projectileSpeed;
        }

        Destroy(projectile, 5f);
    }

    public void OnTakeDamage()
    {
        if (isDead) return;

        if (hitCoroutine != null)
            StopCoroutine(hitCoroutine);

        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        isAttacking = false;
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        currentQuadAnimation = "";

        hitCoroutine = StartCoroutine(HitRoutine());
    }

    IEnumerator HitRoutine()
    {
        isTakingHit = true;
        currentState = AIState.Hit;

        ForcePlayQuadAnimation(hitAnimationName);

        yield return new WaitForSeconds(hitAnimationDuration);

        isTakingHit = false;
        hitCoroutine = null;
        currentQuadAnimation = "";
        GoBackToDefaultState();
    }

    public void OnDeath()
    {
        if (isDead) return;
        if (deathCoroutine != null) return;

        if (hitCoroutine != null)
        {
            StopCoroutine(hitCoroutine);
            hitCoroutine = null;
        }

        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        isAttacking = false;
        isTakingHit = false;
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        currentQuadAnimation = "";

        deathCoroutine = StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        isDead = true;
        currentState = AIState.Dead;

        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        ForcePlayQuadAnimation(dieAnimationName);

        yield return new WaitForSeconds(deathAnimationDuration);
        yield return new WaitForSeconds(disappearDelayAfterDeath);

        gameObject.SetActive(false);
    }

    void HandleFlip()
    {
        float horizontalVel = rb.linearVelocity.x;

        if (Mathf.Abs(horizontalVel) > 0.01f)
            lastNonZeroHorizontal = horizontalVel;
        else if (Mathf.Abs(lookDirection.x) > 0.01f)
            lastNonZeroHorizontal = lookDirection.x;

        float targetYRotation = (lastNonZeroHorizontal < 0f) ? 0f : 180f;
        Vector3 currentEuler = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(currentEuler.x, targetYRotation, currentEuler.z);
    }

    void UpdateQuadAnimation()
    {
        if (quadAnimator == null) return;

        if (isDead || isTakingHit || isAttacking)
            return;

        Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        bool isMoving = horizontalVel.magnitude > 0.05f;

        if (isMoving)
            PlayQuadAnimation(walkAnimationName);
        else
            PlayQuadAnimation(idleAnimationName);
    }

    void PlayQuadAnimation(string animationName)
    {
        if (quadAnimator == null || string.IsNullOrEmpty(animationName)) return;

        if (currentQuadAnimation == animationName)
            return;

        currentQuadAnimation = animationName;
        quadAnimator.Play(animationName, false);
    }

    void ForcePlayQuadAnimation(string animationName)
    {
        if (quadAnimator == null || string.IsNullOrEmpty(animationName)) return;

        currentQuadAnimation = animationName;
        quadAnimator.Play(animationName, true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            StopWaitingCoroutineIfNeeded();
            playerTransform = other.transform;
            currentState = AIState.Chasing;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            if (other.transform == playerTransform)
            {
                playerTransform = null;
                GoBackToDefaultState();
            }
        }
    }

    void GoBackToDefaultState()
    {
        if (isDead) return;

        AIState newState =
            (enemyType == BehaviorType.Patrol && patrolPoints != null && patrolPoints.Length > 0)
            ? AIState.Patrolling
            : AIState.Idle;

        currentState = newState;
    }

    void StopWaitingCoroutineIfNeeded()
    {
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
            isWaitingAtPatrolPoint = false;
        }
    }

    IEnumerator WaitAndMoveToNextPoint()
    {
        isWaitingAtPatrolPoint = true;
        yield return new WaitForSeconds(patrolWaitTime);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        isWaitingAtPatrolPoint = false;
        waitCoroutine = null;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionDistance);

        if (firePoint != null && enemyType == BehaviorType.Ranged)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, rangedStoppingDistance);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, retreatDistance);
        }
    }
}
