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

    [Header("Audio")]
    [SerializeField] private float soundLoopInterval = 0.5f; // Jeda antar suara saat looping

    private Transform playerTransform;
    private Coroutine durationCoroutine;
    private Coroutine soundCoroutine;

    // ── Dipanggil oleh SwirlItem saat di-pickup ─────────────────

    public void Activate(Transform player, float duration)
    {
        playerTransform = player;

        // Aktifkan SwirlPlayer (memicu Awake jika pertama kali)
        gameObject.SetActive(true);

        // Langsung posisikan ke player
        transform.position = playerTransform.position;

        // Hentikan timer & sound sebelumnya (jika ada pick-up ganda)
        if (durationCoroutine != null) StopCoroutine(durationCoroutine);
        if (soundCoroutine != null) StopCoroutine(soundCoroutine);

        durationCoroutine = StartCoroutine(DeactivateAfter(duration));
        soundCoroutine = StartCoroutine(SoundLoop());
    }

    private IEnumerator SoundLoop()
    {
        while (true)
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySound2D("Shiruken");
            
            yield return new WaitForSeconds(soundLoopInterval);
        }
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
        if (soundCoroutine != null)
        {
            StopCoroutine(soundCoroutine);
            soundCoroutine = null;
        }

        durationCoroutine = null;
        gameObject.SetActive(false);
    }

}
