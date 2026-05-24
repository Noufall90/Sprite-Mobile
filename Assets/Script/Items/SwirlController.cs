using System.Collections;
using UnityEngine;

/// <summary>
/// SwirlController — attach ke SwirlPlayer (parent dari 8 shiruken).
/// Script ini SELALU ada di scene. GameObject SwirlPlayer DISET NONAKTIF
/// secara manual di Inspector/Hierarchy.
///
/// Cara kerja:
/// - Saat SwirlItem di-pickup → Activate() dipanggil
/// - SwirlPlayer menjadi aktif, ikut posisi player, dan berputar
/// - Setelah durasi habis → SwirlPlayer dinonaktifkan kembali
///
/// Setup di Inspector:
/// 1. Buat SwirlPlayer GameObject, set NonAktif (uncheck) di Hierarchy
/// 2. Attach script ini ke SwirlPlayer
/// 3. Susun 8 shiruken sebagai child (pastikan punya Collider2D Trigger)
/// 4. Attach SwirlHitbox.cs ke setiap shiruken
/// </summary>
public class SwirlController : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 180f; // derajat per detik

    private Transform playerTransform;
    private Coroutine durationCoroutine;

    // ── Dipanggil oleh SwirlItem saat di-pickup ─────────────────

    public void Activate(Transform player, float duration)
    {
        playerTransform = player;

        // Aktifkan SwirlPlayer (memicu Awake jika pertama kali)
        gameObject.SetActive(true);

        // Langsung posisikan ke player
        transform.position = playerTransform.position;

        // Hentikan timer sebelumnya (jika ada pick-up ganda)
        if (durationCoroutine != null)
            StopCoroutine(durationCoroutine);

        durationCoroutine = StartCoroutine(DeactivateAfter(duration));
    }

    // ── Lifecycle ────────────────────────────────────────────────

    private void Update()
    {
        if (playerTransform == null) return;

        // Ikuti posisi player setiap frame
        transform.position = playerTransform.position;

        // Putar seluruh parent → semua shiruken anak ikut berputar
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    private IEnumerator DeactivateAfter(float duration)
    {
        yield return new WaitForSeconds(duration);

        // Nonaktifkan kembali setelah durasi habis
        durationCoroutine = null;
        gameObject.SetActive(false);
    }

}
