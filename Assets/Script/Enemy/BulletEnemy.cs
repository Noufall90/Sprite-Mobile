using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
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
        Rigidbody2D rb =
            GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;

        Collider2D col =
            GetComponent<Collider2D>();

        col.isTrigger = true;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // HIT PLAYER ONLY
        if (!other.CompareTag("Player"))
            return;

        HealthPlayer playerHealth =
            other.GetComponent<HealthPlayer>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }

        if (destroyOnHit)
        {
            Destroy(gameObject);
        }
    }
}