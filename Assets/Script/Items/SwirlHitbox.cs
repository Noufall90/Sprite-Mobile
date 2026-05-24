using UnityEngine;

/// <summary>
/// SwirlHitbox — attach ke setiap shiruken (child dari SwirlPlayer).
/// Mendeteksi collision dengan enemy bertag "Enemy" dan memberikan damage.
/// Damage diatur langsung di script ini via Inspector.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SwirlHitbox : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 10;

    private void Awake()
    {
        // Pastikan collider bertipe trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        EnemyHealth eh = other.GetComponent<EnemyHealth>();
        if (eh == null) eh = other.GetComponentInParent<EnemyHealth>();
        if (eh != null) eh.TakeDamage(damage);
    }
}
