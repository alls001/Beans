using UnityEngine;
using System.Collections;

public class HealthSystem : MonoBehaviour
{
    [Header("Configuracao")]
    public float maxHealth = 10f;
    public float disappearDelayAfterDeath = 0.8f;

    [Header("Knockback")]
    public float knockbackForce = 8f;
    public float knockbackDuration = 0.18f;

    [Header("Efeitos de Dano")]
    public float invulnerabilityTime = 0.6f;
    public float blinkInterval = 0.08f;

    public float CurrentHealth { get; private set; }

    private bool isDead = false;
    private bool isInvulnerable = false;
    private bool isKnockedBack = false;

    private Animator animator;
    private Rigidbody rb;
    private Collider charCollider;
    private SpriteRenderer spriteRenderer;
    private PlayerController3D playerController;
    private EnemyBase enemyController;
    private BossFootController bossController;

    void Awake()
    {
        CurrentHealth = maxHealth;

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        charCollider = GetComponent<Collider>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerController = GetComponent<PlayerController3D>();
        enemyController = GetComponent<EnemyBase>();
        bossController = GetComponent<BossFootController>();

        bool isBoss = bossController != null;

        if (animator == null) Debug.LogWarning("HealthSystem: Animator nao encontrado em " + gameObject.name);
        if (rb == null && !isBoss) Debug.LogWarning("HealthSystem: Rigidbody nao encontrado em " + gameObject.name);
        if (charCollider == null && !isBoss) Debug.LogWarning("HealthSystem: Collider nao encontrado em " + gameObject.name);
        if (spriteRenderer == null) Debug.LogWarning("HealthSystem: SpriteRenderer nao encontrado em " + gameObject.name);
    }

    public void TakeDamage(float damageAmount, Transform attackerTransform = null)
    {
        if (isDead) return;
        if (isInvulnerable) return;

        CurrentHealth -= damageAmount;
        Debug.Log($"{gameObject.name} tomou {damageAmount} de dano. Vida: {CurrentHealth}/{maxHealth}");

        bool isPlayer = playerController != null;
        bool isEnemy = enemyController != null;
        bool isBoss = bossController != null;

        if (attackerTransform != null && knockbackForce > 0f && rb != null && !rb.isKinematic)
        {
            StartCoroutine(ApplyKnockbackCoroutine(attackerTransform));
        }

        if (spriteRenderer != null)
        {
            StartCoroutine(DamageFlashAndInvulnerability());
        }
        else
        {
            StartCoroutine(SimpleInvulnerability());
        }

        if (CurrentHealth > 0f)
        {
            if (isPlayer)
            {
                playerController.OnTakeDamage();
            }
            else if (isEnemy)
            {
                enemyController.OnTakeDamage();
            }
            else if (isBoss)
            {
                bossController.OnBossDamaged(damageAmount, CurrentHealth, maxHealth);
            }
            else if (animator != null)
            {
                animator.SetTrigger("Damage");
            }
        }

        if (CurrentHealth <= 0f)
        {
            CurrentHealth = 0f;

            if (!isDead)
            {
                isDead = true;

                if (isPlayer)
                {
                    playerController.OnDeath();
                }
                else if (isEnemy)
                {
                    enemyController.OnDeath();
                }
                else if (isBoss)
                {
                    bossController.OnBossDeath();
                }
                else
                {
                    StartCoroutine(HandleGenericDeath());
                }
            }
        }
    }

    private IEnumerator ApplyKnockbackCoroutine(Transform attacker)
    {
        if (rb == null || attacker == null) yield break;

        isKnockedBack = true;

        Vector3 dir = transform.position - attacker.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f)
        {
            dir = transform.forward;
            dir.y = 0f;
        }

        dir.Normalize();

        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        rb.AddForce(dir * knockbackForce, ForceMode.VelocityChange);

        float t = 0f;
        while (t < knockbackDuration)
        {
            t += Time.deltaTime;
            yield return null;
        }

        isKnockedBack = false;
    }

    private IEnumerator DamageFlashAndInvulnerability()
    {
        if (spriteRenderer == null) yield break;

        isInvulnerable = true;
        Color original = spriteRenderer.color;
        float elapsed = 0f;
        bool visible = true;

        while (elapsed < invulnerabilityTime)
        {
            spriteRenderer.color = visible ? original : new Color(1f, 0.3f, 0.3f, 0.8f);
            visible = !visible;
            elapsed += blinkInterval;
            yield return new WaitForSeconds(blinkInterval);
        }

        spriteRenderer.color = original;
        isInvulnerable = false;
    }

    private IEnumerator SimpleInvulnerability()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityTime);
        isInvulnerable = false;
    }

    private IEnumerator HandleGenericDeath()
    {
        Debug.Log(gameObject.name + " morreu.");

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (animator != null)
        {
            foreach (var p in animator.parameters)
            {
                if (p.name == "Die")
                {
                    animator.SetTrigger("Die");
                    break;
                }
            }

            float enterTimeout = 0.5f;
            float waited = 0f;

            while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Die") && waited < enterTimeout)
            {
                waited += Time.deltaTime;
                yield return null;
            }

            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Die"))
            {
                while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
                    yield return null;
            }

            yield return new WaitForSeconds(disappearDelayAfterDeath);
        }

        if (charCollider != null)
            charCollider.enabled = false;

        if (rb != null)
            rb.isKinematic = true;

        gameObject.SetActive(false);
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        if (amount <= 0f) return;

        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
        Debug.Log($"{gameObject.name} curou {amount}. Vida: {CurrentHealth}/{maxHealth}");
    }

    public void ResetHealthToMax()
    {
        CurrentHealth = maxHealth;
        isDead = false;
        isInvulnerable = false;
        isKnockedBack = false;
    }

    public bool IsDead() => isDead;
    public bool IsInvulnerable() => isInvulnerable;
    public bool IsKnockedBack() => isKnockedBack;
}
