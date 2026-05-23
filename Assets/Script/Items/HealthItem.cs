using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HealthItem : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private int healAmount = 10;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private AudioClip pickupSound;

    private void Reset()
    {
        // Ensure collider is set as trigger by default when added
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        HealthPlayer hp = other.GetComponent<HealthPlayer>();
        if (hp != null)
        {
            hp.Heal(healAmount);
        }
        else
        {
            Debug.LogWarning($"HealthItem: Player does not have HealthPlayer component on {other.name}");
        }

        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        Destroy(gameObject);
    }
}
