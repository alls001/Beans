using UnityEngine;

public class HealingHeart : MonoBehaviour
{
    [Header("Cura")]
    public float healAmount = 2f;
    public string targetTag = "Player";
    public float lifeTime = 10f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(targetTag))
            return;

        HealthSystem hs = other.GetComponent<HealthSystem>();
        if (hs == null)
            hs = other.GetComponentInParent<HealthSystem>();

        if (hs != null)
        {
            hs.Heal(healAmount);
        }

        Destroy(gameObject);
    }
}