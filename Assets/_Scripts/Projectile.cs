using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage = 1f;
    public string targetTag = "Player";
    public LayerMask targetLayers;
    public float lifeTime = 5f;

    private Rigidbody rb;
    private Transform owner;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
            Debug.LogError("Projectile sem Rigidbody!");

        Destroy(gameObject, lifeTime);
    }

    public void Launch(Vector3 velocity, float projectileDamage, string projectileTargetTag, LayerMask projectileTargetLayers, Transform projectileOwner, float projectileLifeTime)
    {
        rb = GetComponent<Rigidbody>();

        damage = projectileDamage;
        targetTag = projectileTargetTag;
        targetLayers = projectileTargetLayers;
        owner = projectileOwner;

        if (projectileLifeTime > 0f)
            lifeTime = projectileLifeTime;

        if (velocity.sqrMagnitude > 0.0001f)
            transform.forward = velocity.normalized;

        if (rb != null)
        {
            if (rb.isKinematic)
                rb.isKinematic = false;

            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.linearVelocity = velocity;
        }

        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsOwnerCollider(other))
            return;

        if (IsTarget(other))
        {
            HealthSystem targetHealth = other.GetComponent<HealthSystem>();
            if (targetHealth == null)
                targetHealth = other.GetComponentInParent<HealthSystem>();

            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage, transform);
            }

            Destroy(gameObject);
            return;
        }

        // Bateu em parede ou chão
        if (other.GetComponentInParent<HealthSystem>() != null)
            return;

        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }

    private bool IsTarget(Collider other)
    {
        bool layerMatches =
            targetLayers.value != 0 &&
            (targetLayers.value & (1 << other.gameObject.layer)) != 0;

        bool tagMatches =
            !string.IsNullOrEmpty(targetTag) &&
            other.CompareTag(targetTag);

        return layerMatches || tagMatches;
    }

    private bool IsOwnerCollider(Collider other)
    {
        if (owner == null)
            return false;

        return other.transform == owner || other.transform.IsChildOf(owner);
    }
}
