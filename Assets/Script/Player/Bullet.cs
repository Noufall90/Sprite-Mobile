using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 10;
    public float lifeTime = 5f;

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

        if (col == null)
            return;

        // Hit enemy: apply damage then destroy
        if (col.CompareTag("Enemy"))
        {
            var enemyHealth = col.GetComponentInParent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }

            Destroy(gameObject);
            return;
        }

        // Hit wall: just destroy
        if (col.CompareTag("Wall"))
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Support trigger-based bullets as well (in case prefab uses trigger)
        if (other == null)
            return;

        if (other.CompareTag("Enemy"))
        {
            var enemyHealth = other.GetComponentInParent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
            return;
        }
    }
}