using UnityEngine;
using System.Collections;

public class HealthSystem : MonoBehaviour
{
    [Header("Configuração")]
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
    private Coroutine knockbackCoroutine;

    void Awake()
    {
        CurrentHealth = maxHealth;

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        charCollider = GetComponent<Collider>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerController = GetComponent<PlayerController3D>();
        enemyController = GetComponent<EnemyBase>();

        if (animator == null) Debug.LogWarning("HealthSystem: Animator não encontrado em " + gameObject.name);
        if (rb == null) Debug.LogWarning("HealthSystem: Rigidbody não encontrado em " + gameObject.name);
        if (charCollider == null) Debug.LogWarning("HealthSystem: Collider não encontrado em " + gameObject.name);
        if (spriteRenderer == null) Debug.LogWarning("HealthSystem: SpriteRenderer não encontrado em " + gameObject.name);
    }

    public void TakeDamage(float damageAmount, Transform attackerTransform = null)
    {
        if (isDead) return;
        if (isInvulnerable) return;

        CurrentHealth -= damageAmount;
        Debug.Log($"{gameObject.name} tomou {damageAmount} de dano. Vida: {CurrentHealth}/{maxHealth}");

        bool isPlayer = playerController != null;
        bool isEnemy = enemyController != null;

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
            else if (animator != null)
            {
                animator.SetTrigger("Damage");
            }
        }

        if (attackerTransform != null && knockbackForce > 0f && rb != null && !rb.isKinematic)
        {
            if (knockbackCoroutine != null)
                StopCoroutine(knockbackCoroutine);

            knockbackCoroutine = StartCoroutine(ApplyKnockbackCoroutine(attackerTransform));
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
            dir = attacker.forward;
            dir.y = 0f;
        }

        dir.Normalize();

        float t = 0f;
        while (t < knockbackDuration)
        {
            rb.linearVelocity = new Vector3(dir.x * knockbackForce, rb.linearVelocity.y, dir.z * knockbackForce);
            t += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        isKnockedBack = false;
        knockbackCoroutine = null;
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
    public bool IsDead() => isDead;
    public bool IsInvulnerable() => isInvulnerable;
    public bool IsKnockedBack() => isKnockedBack;
}
