using System.Collections;
using UnityEngine;

/// <summary>
/// DamageItem — ketika player mengambil item ini:
/// - Damage Shoot.damage player menjadi 2x lipat selama damageTime detik
/// - Setelah habis, dikembalikan ke nilai semula
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DamageItem : MonoBehaviour
{
    [Header("Damage Boost Settings")]
    [SerializeField] private float damageTime = 5f;
    [SerializeField] private float damageMultiplier = 2f;
    [SerializeField] private string playerTag = "Player";

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        HealthPlayer hp = other.GetComponent<HealthPlayer>();
        if (hp == null) hp = other.GetComponentInParent<HealthPlayer>();
        if (hp == null) return;

        // Cari Shoot di player atau child-nya
        Shoot shoot = other.GetComponentInParent<Shoot>();
        if (shoot == null) shoot = other.GetComponent<Shoot>();
        if (shoot == null) shoot = other.GetComponentInChildren<Shoot>();
        if (shoot == null) return;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound2D("PickUpItem");

        hp.StartCoroutine(DamageBoostRoutine(shoot));

        Destroy(gameObject);
    }

    private IEnumerator DamageBoostRoutine(Shoot shoot)
    {
        int originalDamage = shoot.damage;
        shoot.damage = Mathf.RoundToInt(originalDamage * damageMultiplier);

        yield return new WaitForSeconds(damageTime);

        if (shoot != null)
            shoot.damage = originalDamage;
    }
}
