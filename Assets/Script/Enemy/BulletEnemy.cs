using UnityEngine;

public class BulletEnemy : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 10;

    [Header("Lifetime")]
    [SerializeField] private float lifeTime = 5f;

    [Header("Impact")]
    [SerializeField] private bool destroyOnHit = true;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        
        if (col != null)
        {
            col.isTrigger = false;
        }
        else
        {
            Debug.LogWarning($"BulletEnemy missing Collider2D: {gameObject.name}");
        }
    }

    private void Start()
    {
        if (lifeTime > 0f)
        {
            Destroy(gameObject, lifeTime);
        }
        else if (lifeTime == 0f)
        {
            // Jika lifetime 0, destroy segera
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var col = collision.collider;

        // Hit player
        if (col.CompareTag("Player"))
        {
            var playerHealth = col.GetComponentInParent<HealthPlayer>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            if (destroyOnHit)
                Destroy(gameObject);

            return;
        }

        // Hit wall -> destroy bullet
        if (col.CompareTag("Wall"))
        {
            if (destroyOnHit)
                Destroy(gameObject);
            return;
        }
    }
}