using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody), typeof(Animator))]
public class PlayerController3D : MonoBehaviour
{
    [Header("Movimento")]
    public float moveSpeed = 5f;
    public float sprintForce = 12f;
    public float sprintDuration = 0.25f;
    public float sprintCooldown = 1.2f;

    [Header("Combate")]
    public Transform attackPoint;
    public LayerMask enemyLayer;

    [Header("Combo")]
    public float comboResetTime = 0.9f;

    [Header("Ataque 1")]
    public float attack1Damage = 2f;
    public float attack1Range = 1.3f;
    public float attack1Duration = 0.35f;

    [Header("Ataque 2")]
    public float attack2Damage = 4f;
    public float attack2Range = 1.5f;
    public float attack2Duration = 0.4f;

    [Header("Ataque 3")]
    public float attack3Damage = 6f;
    public float attack3Range = 2.5f;
    public float attack3Duration = 0.55f;
    public bool attack3Hits360 = true;

    [Header("Hit / Death")]
    public float hitAnimationDuration = 0.35f;
    public float deathAnimationDuration = 1.2f;
    public GameObject gameOverPanel;

    [Header("Impacto")]
    public float hitStopDuration = 0.05f;

    [Header("Áudio")]
    public List<AudioSource> audioSource;
    public List<AudioClip> audioClips;

    [Header("Visual Quad")]
    public QuadSpriteAnimator quadAnimator;

    [Header("Nomes das animações do Quad")]
    public string idleAnimationName = "Player_Idle";
    public string walkEastAnimationName = "Player_Walk_E";
    public string walkNorthEastAnimationName = "Player_Walk_NE";
    public string walkSouthAnimationName = "Player_Walk_S";
    public string sprintAnimationName = "Player_Sprint";
    public string attack1AnimationName = "Player_Attack1";
    public string attack2AnimationName = "Player_Attack2";
    public string attack3AnimationName = "Player_Attack3";
    public string hitAnimationName = "Player_Hit";
    public string dieAnimationName = "Player_Die";

    private Rigidbody rb;
    private Animator animator;

    private Vector2 rawInput;
    private Vector2 moveInput;

    private bool canSprint = true;
    private bool isSprinting = false;
    private bool isAttacking = false;
    private bool isTakingHit = false;
    private bool isDead = false;
    private bool hitStopRunning = false;
    private bool lockMovementDuringAttack = false;

    private float lastNonZeroHorizontal = 1f;

    private int comboStep = 0;
    private float lastComboTime = -999f;
    private bool queuedNextAttack = false;
    private bool canQueueNextAttack = false;

    private Coroutine comboCoroutine;
    private Coroutine hitCoroutine;
    private Coroutine deathCoroutine;

    private string currentQuadAnimation = "";

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        if (quadAnimator == null)
            quadAnimator = GetComponentInChildren<QuadSpriteAnimator>();

        if (attackPoint == null)
            Debug.LogError("PlayerController3D: AttackPoint não atribuído!");

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        ForcePlayQuadAnimation(idleAnimationName);
    }

    void Update()
    {
        HandleMovementInput();
        HandleSprintInput();
        HandleAttackInput();
        HandleFlip();
        HandleWalkSound();
        UpdateAnimator();
        UpdateQuadAnimation();
        HandleComboReset();
    }

    void FixedUpdate()
    {
        if (isDead || isTakingHit)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        if (!isSprinting && (!isAttacking || !lockMovementDuringAttack) && !hitStopRunning)
        {
            MoveCharacter();
        }
        else if ((isAttacking && lockMovementDuringAttack) || hitStopRunning)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }

    void HandleMovementInput()
    {
        if (isDead || isTakingHit)
        {
            rawInput = Vector2.zero;
            moveInput = Vector2.zero;
            return;
        }

        rawInput.x = Input.GetAxisRaw("Horizontal");
        rawInput.y = Input.GetAxisRaw("Vertical");

        moveInput = rawInput;

        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();
    }

    void HandleSprintInput()
    {
        if (isDead || isTakingHit) return;

        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Space)) && canSprint && !isAttacking)
        {
            StartCoroutine(SprintDash());
        }
    }

    void HandleAttackInput()
    {
        if (isDead || isTakingHit) return;

        if (Input.GetButtonDown("Fire1") && !isSprinting)
        {
            if (!isAttacking)
            {
                if (comboCoroutine == null)
                    comboCoroutine = StartCoroutine(AttackComboSequence());
            }
            else if (canQueueNextAttack)
            {
                queuedNextAttack = true;
            }
        }
    }

    void HandleComboReset()
    {
        if (isDead || isTakingHit) return;

        if (!isAttacking && comboCoroutine == null && Time.time - lastComboTime > comboResetTime)
        {
            comboStep = 0;
            queuedNextAttack = false;
            canQueueNextAttack = false;
        }
    }

    void HandleFlip()
    {
        if (isDead) return;

        if (Mathf.Abs(rawInput.x) > 0.01f)
        {
            lastNonZeroHorizontal = rawInput.x;

            float targetYRotation = rawInput.x > 0 ? 0f : 180f;
            transform.rotation = Quaternion.Euler(0f, targetYRotation, 0f);
        }
        else
        {
            float targetYRotation = lastNonZeroHorizontal > 0 ? 0f : 180f;
            Quaternion targetRotation = Quaternion.Euler(0f, targetYRotation, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    IEnumerator SprintDash()
    {
        canSprint = false;
        isSprinting = true;
        animator.SetBool("IsSprinting", true);
        ForcePlayQuadAnimation(sprintAnimationName);

        if (audioSource != null && audioSource.Count > 3 && audioSource[3] != null)
            audioSource[3].Play();

        Vector3 dashDir = new Vector3(moveInput.x, 0f, moveInput.y);

        if (dashDir.sqrMagnitude < 0.001f)
            dashDir = new Vector3(lastNonZeroHorizontal > 0 ? 1f : -1f, 0f, 0f);

        dashDir.Normalize();

        rb.linearVelocity = new Vector3(dashDir.x * sprintForce, rb.linearVelocity.y, dashDir.z * sprintForce);

        yield return new WaitForSeconds(sprintDuration);

        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);

        isSprinting = false;
        animator.SetBool("IsSprinting", false);
        ForcePlayQuadAnimation(idleAnimationName);

        yield return new WaitForSeconds(sprintCooldown);
        canSprint = true;
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        float speed = Mathf.Clamp01(moveInput.magnitude);
        float effectiveSpeed = (isAttacking || isSprinting || isTakingHit || isDead) ? 0f : speed;

        animator.SetFloat("Horizontal", Mathf.Abs(moveInput.x));
        animator.SetFloat("Vertical", moveInput.y);
        animator.SetFloat("Speed", effectiveSpeed);
    }

    string GetDirectionalWalkAnimation()
{
    float x = rawInput.x;
    float y = rawInput.y;

    // Andando para baixo
    if (y < -0.3f)
        return walkSouthAnimationName;

    // Andando para cima, mesmo sem componente horizontal
    if (y > 0.3f)
        return walkNorthEastAnimationName;

    // Andando para os lados
    if (Mathf.Abs(x) > 0.1f)
        return walkEastAnimationName;

    return idleAnimationName;
}

    void UpdateQuadAnimation()
    {
        if (quadAnimator == null) return;
        if (isAttacking || isTakingHit || isDead) return;

        bool isMoving = moveInput.sqrMagnitude > 0.01f;

        if (isSprinting)
        {
            PlayQuadAnimation(sprintAnimationName);
            return;
        }

        if (isMoving)
        {
            PlayQuadAnimation(GetDirectionalWalkAnimation());
        }
        else
        {
            PlayQuadAnimation(idleAnimationName);
        }
    }

    void MoveCharacter()
    {
        Vector3 target = new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed;
        target.y = rb.linearVelocity.y;
        rb.linearVelocity = target;
    }

    IEnumerator AttackComboSequence()
    {
        isAttacking = true;
        canQueueNextAttack = false;
        queuedNextAttack = false;
        lockMovementDuringAttack = true;

        comboStep++;
        if (comboStep > 3)
            comboStep = 1;

        lastComboTime = Time.time;

        switch (comboStep)
        {
            case 1:
                lockMovementDuringAttack = true;
                animator.ResetTrigger("Attack2");
                animator.ResetTrigger("Attack3");
                animator.SetTrigger("Attack1");
                ForcePlayQuadAnimation(attack1AnimationName);
                PlayAttackSound(1);
                break;

            case 2:
                lockMovementDuringAttack = true;
                animator.ResetTrigger("Attack1");
                animator.ResetTrigger("Attack3");
                animator.SetTrigger("Attack2");
                ForcePlayQuadAnimation(attack2AnimationName);
                PlayAttackSound(1);
                break;

            case 3:
                lockMovementDuringAttack = false;
                animator.ResetTrigger("Attack1");
                animator.ResetTrigger("Attack2");
                animator.SetTrigger("Attack3");
                ForcePlayQuadAnimation(attack3AnimationName);
                PlayAttackSound(1);
                break;
        }

        yield return new WaitUntil(() => !isAttacking);

        canQueueNextAttack = false;
        lockMovementDuringAttack = false;

        bool shouldContinue = queuedNextAttack;

        if (comboStep == 3)
            comboStep = 0;

        comboCoroutine = null;

        if (shouldContinue && !isDead)
            comboCoroutine = StartCoroutine(AttackComboSequence());
    }

    void ResetCombatState()
    {
        queuedNextAttack = false;
        canQueueNextAttack = false;
        comboStep = 0;
        isAttacking = false;
        isSprinting = false;
        lockMovementDuringAttack = false;

        if (comboCoroutine != null)
        {
            StopCoroutine(comboCoroutine);
            comboCoroutine = null;
        }

        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);

        animator.ResetTrigger("Attack1");
        animator.ResetTrigger("Attack2");
        animator.ResetTrigger("Attack3");
        animator.SetBool("IsSprinting", false);

        currentQuadAnimation = "";
    }

    public void OnTakeDamage()
    {
        if (isDead) return;

        if (hitCoroutine != null)
            StopCoroutine(hitCoroutine);

        ResetCombatState();
        hitCoroutine = StartCoroutine(HitReactionRoutine());
    }

    IEnumerator HitReactionRoutine()
    {
        isTakingHit = true;
        ForcePlayQuadAnimation(hitAnimationName);

        yield return new WaitForSeconds(hitAnimationDuration);

        isTakingHit = false;
        hitCoroutine = null;
        currentQuadAnimation = "";
    }

    public void OnDeath()
    {
        if (isDead) return;
        if (deathCoroutine != null) return;

        ResetCombatState();
        deathCoroutine = StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        isDead = true;
        isTakingHit = false;

        rb.linearVelocity = Vector3.zero;
        ForcePlayQuadAnimation(dieAnimationName);

        yield return new WaitForSeconds(deathAnimationDuration);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("PlayerController3D: GameOverPanel não atribuído.");
        }

        deathCoroutine = null;
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

    void PlayAttackSound(int index)
    {
        if (audioSource == null) return;
        if (audioSource.Count > index && audioSource[index] != null)
            audioSource[index].Play();
    }

    void PerformDamageCheck(float damage, float range, bool areaAttack)
    {
        if (attackPoint == null) return;

        Collider[] hitColliders = Physics.OverlapSphere(attackPoint.position, range, enemyLayer);
        HashSet<HealthSystem> hitTargets = new HashSet<HealthSystem>();

        bool hitSomeone = false;

        foreach (Collider c in hitColliders)
        {
            HealthSystem hs = c.GetComponent<HealthSystem>();
            if (hs == null)
                hs = c.GetComponentInParent<HealthSystem>();

            BossFootController bossTarget = hs != null ? hs.GetComponent<BossFootController>() : null;

            if (!areaAttack && bossTarget == null)
            {
                Vector3 dirToEnemy = (c.transform.position - transform.position).normalized;
                float dot = Vector3.Dot(transform.forward, dirToEnemy);

                if (dot < 0.3f)
                    continue;
            }

            if (hs != null && hitTargets.Add(hs))
            {
                hs.TakeDamage(damage);
                hitSomeone = true;
            }
        }

        if (hitSomeone)
            StartCoroutine(HitStopCoroutine());
    }

    IEnumerator HitStopCoroutine()
    {
        if (hitStopRunning) yield break;

        hitStopRunning = true;

        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(hitStopDuration);

        Time.timeScale = originalTimeScale;
        hitStopRunning = false;
    }

    public void HitAttack1()
    {
        PerformDamageCheck(attack1Damage, attack1Range, false);
    }

    public void HitAttack2()
    {
        PerformDamageCheck(attack2Damage, attack2Range, false);
    }

    public void HitAttack3()
    {
        PerformDamageCheck(attack3Damage, attack3Range, attack3Hits360);
    }

    public void OpenComboWindow()
    {
        canQueueNextAttack = true;
    }

    public void CloseComboWindow()
    {
        canQueueNextAttack = false;
    }

    public void EndAttack()
    {
        isAttacking = false;
    }

    void HandleWalkSound()
    {
        if (audioSource == null || audioSource.Count == 0) return;
        if (audioSource[0] == null) return;

        AudioSource walkSource = audioSource[0];

        bool isMoving =
            !isSprinting &&
            !isAttacking &&
            !isTakingHit &&
            !isDead &&
            (moveInput.sqrMagnitude > 0.01f || rb.linearVelocity.magnitude > 0.1f);

        if (isMoving && !walkSource.isPlaying)
        {
            walkSource.loop = true;
            if (walkSource.clip != null) walkSource.Play();
        }
        else if (!isMoving && walkSource.isPlaying)
        {
            walkSource.Stop();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPoint.position, attack1Range);

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(attackPoint.position, attack2Range);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attack3Range);
    }
}
