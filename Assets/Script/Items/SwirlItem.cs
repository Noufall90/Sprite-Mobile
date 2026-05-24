using System;
using UnityEngine;

/// <summary>
/// SwirlItem — item pickup di scene.
/// Saat di-pickup: tembak static event OnSwirlStart ke SwirlConnection.
/// Tidak perlu referensi apapun ke GameObject di scene.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SwirlItem : MonoBehaviour
{
    [Header("Swirl Settings")]
    [SerializeField] private float  swirlTime  = 5f;
    [SerializeField] private string playerTag  = "Player";

    // Static event — didengar oleh SwirlConnection yang selalu aktif di scene
    public static event Action<Transform, float> OnSwirlStart;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound2D("PickUpItem");

        // Broadcast ke SwirlConnection: player transform + durasi
        OnSwirlStart?.Invoke(other.transform.root, swirlTime);

        Destroy(gameObject);
    }
}
