using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public abstract class EnemyBase : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] protected Transform playerTransform;
    [SerializeField] protected QuadSpriteAnimator quadAnimator;

    [Header("Detecção")]
    public bool autoDetectPlayer = true;
    public float detectionDistance = 20f;
    public LayerMask playerLayer;

    [Header("Movimento")]
    public float moveSpeed = 3.5f;
    public float stoppingDistance = 0.9f;
    public bool useGravity = true;

    [Header("Hit / Death")]
    public float hitDuration = 0.35f;
    public float deathDuration = 0.9f;
    public float disableDelayAfterDeath = 0.5f;

    [Header("Animações")]
    public string idleAnimationName = "Idle";
    public string walkAnimationName = "walk";
    public string hitAnimationName = "Hit";
    public string dieAnimationName = "Die";

    protected Rigidbody rb;

    protected bool isDead = false;
    protected bool isTakingHit = false;
    protected bool isAttacking = false;

    protected Vector3 moveDirection = Vector3.zero;
    protected float moveSpeedCurrent = 0f;

    protected Vector2 lookDirection = Vector2.right;
    protected float lastNonZeroHorizontal = 1f;

    protected Coroutine hitRoutine;
    protected Coroutine deathRoutine;

    protected string currentQuadAnimation = "";

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (quadAnimator == null)
            quadAnimator = GetComponentInChildren<QuadSpriteAnimator>();

        if (playerTransform == null && autoDetectPlayer)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;
        }

        ForcePlayQuadAnimation(idleAnimationName);
    }

    protected virtual void Update()
    {
        if (isDead) return;

        if (playerTransform == null && autoDetectPlayer)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;
        }

        UpdateLookDirection();

        if (!isDead && !isTakingHit)
            UpdateBehavior();

        HandleFlip();
        UpdateQuadAnimation();
    }

    protected virtual void FixedUpdate()
    {
        if (rb == null) return;

        if (isDead || isTakingHit)
        {
            StopMovement();
            ApplyMovement();
            return;
        }

        ApplyMovement();
    }

    protected abstract void UpdateBehavior();

    protected void SetMovement(Vector3 direction, float speed)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.0001f)
            direction.Normalize();

        moveDirection = direction;
        moveSpeedCurrent = speed;
    }

    protected void StopMovement()
    {
        moveDirection = Vector3.zero;
        moveSpeedCurrent = 0f;
    }

    protected void ApplyMovement()
    {
        Vector3 velocity = moveDirection * moveSpeedCurrent;
        velocity.y = useGravity ? rb.linearVelocity.y : 0f;
        rb.linearVelocity = velocity;
    }

    protected float DistanceToPlayerXZ()
    {
        if (playerTransform == null) return Mathf.Infinity;

        Vector3 a = transform.position;
        Vector3 b = playerTransform.position;
        a.y = 0f;
        b.y = 0f;

        return Vector3.Distance(a, b);
    }

    protected void UpdateLookDirection()
    {
        if (playerTransform == null) return;

        Vector3 toPlayer = playerTransform.position - transform.position;
        toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude > 0.0001f)
            lookDirection = new Vector2(toPlayer.x, toPlayer.z).normalized;
    }

    protected void HandleFlip()
    {
        float horizontal = moveDirection.x;

        if (Mathf.Abs(horizontal) > 0.01f)
            lastNonZeroHorizontal = horizontal;
        else if (Mathf.Abs(lookDirection.x) > 0.01f)
            lastNonZeroHorizontal = lookDirection.x;

        float targetY = lastNonZeroHorizontal < 0f ? 0f : 180f;
        Vector3 euler = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(euler.x, targetY, euler.z);
    }

    protected void UpdateQuadAnimation()
    {
        if (quadAnimator == null) return;
        if (isDead || isTakingHit || isAttacking) return;

        bool moving = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).sqrMagnitude > 0.01f;

        if (moving)
            PlayQuadAnimation(walkAnimationName);
        else
            PlayQuadAnimation(idleAnimationName);
    }

    protected void PlayQuadAnimation(string animationName)
    {
        if (quadAnimator == null || string.IsNullOrEmpty(animationName)) return;
        if (currentQuadAnimation == animationName) return;

        currentQuadAnimation = animationName;
        quadAnimator.Play(animationName, false);
    }

    protected void ForcePlayQuadAnimation(string animationName)
    {
        if (quadAnimator == null || string.IsNullOrEmpty(animationName)) return;

        currentQuadAnimation = animationName;
        quadAnimator.Play(animationName, true);
    }

    public virtual void OnTakeDamage()
    {
        if (isDead) return;

        if (hitRoutine != null)
            StopCoroutine(hitRoutine);

        isAttacking = false;
        StopMovement();

        hitRoutine = StartCoroutine(HitRoutine());
    }

    protected virtual IEnumerator HitRoutine()
    {
        isTakingHit = true;
        ForcePlayQuadAnimation(hitAnimationName);

        yield return new WaitForSeconds(hitDuration);

        isTakingHit = false;
        hitRoutine = null;
        currentQuadAnimation = "";
    }

    public virtual void OnDeath()
    {
        if (isDead) return;
        if (deathRoutine != null) return;

        isAttacking = false;
        isTakingHit = false;
        StopMovement();

        deathRoutine = StartCoroutine(DeathRoutine());
    }

    protected virtual IEnumerator DeathRoutine()
    {
        isDead = true;

        StopMovement();
        ApplyMovement();

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        ForcePlayQuadAnimation(dieAnimationName);

        yield return new WaitForSeconds(deathDuration);
        yield return new WaitForSeconds(disableDelayAfterDeath);

        gameObject.SetActive(false);
    }
}