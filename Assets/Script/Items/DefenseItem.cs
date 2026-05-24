using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// DefenseItem — item yang di-pickup oleh player.
/// Setelah di-pickup dan di-Destroy, defense tetap berjalan karena
/// coroutine berjalan di HealthPlayer, dan komunikasi ke canvas
/// dilakukan via static event (OnDefenseStart / OnDefenseEnd).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DefenseItem : MonoBehaviour
{
    [Header("Defense Settings")]
    [SerializeField] private float defTime = 5f;
    [SerializeField] private string playerTag = "Player";

    // Flag static — dibaca oleh HealthPlayer.TakeDamage() untuk block damage
    public static bool IsDefenseActive { get; private set; } = false;

    // Static events — didengar oleh DefenseCanvas yang selalu ada di scene
    public static event Action OnDefenseStart;
    public static event Action OnDefenseEnd;

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

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound2D("PickUpItem");

        // Jalankan coroutine di HealthPlayer (tetap hidup setelah item destroy)
        hp.StartCoroutine(DefenseRoutine());

        Destroy(gameObject);
    }

    private IEnumerator DefenseRoutine()
    {
        IsDefenseActive = true;
        OnDefenseStart?.Invoke(); // << beritahu DefenseCanvas untuk tampil

        yield return new WaitForSeconds(defTime);

        IsDefenseActive = false;
        OnDefenseEnd?.Invoke();   // << beritahu DefenseCanvas untuk sembunyi
    }
}
