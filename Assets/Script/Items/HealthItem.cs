using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HealthItem : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private int healAmount = 10;
    [SerializeField] private string playerTag = "Player";

    private void Reset()
    {
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

            // Play pickup sound
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySound2D("PickUpItem");
            }

            Destroy(gameObject);
        }
    }
}