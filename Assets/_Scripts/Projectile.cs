using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage = 1f;
    public string targetTag = "Player";
    public float lifeTime = 5f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
            Debug.LogError("Projectile sem Rigidbody!");

        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // Evita bater em si mesmo ou no inimigo
        if (other.CompareTag(targetTag))
        {
            HealthSystem targetHealth = other.GetComponent<HealthSystem>();

            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage, transform);
            }

            Destroy(gameObject);
            return;
        }

        // Bateu em parede ou chão
        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}