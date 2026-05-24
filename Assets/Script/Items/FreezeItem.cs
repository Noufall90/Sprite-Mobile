using System.Collections;
using UnityEngine;

/// <summary>
/// FreezeItem — ketika player mengambil item ini:
/// - Semua enemy di scene akan berhenti bergerak dan menyerang selama frezeTime detik
/// - Cara freeze: disable EnemyAI, EnemyFSM, dan EnemyShoot di semua enemy aktif
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class FreezeItem : MonoBehaviour
{
    [Header("Freeze Settings")]
    [SerializeField] private float frezeTime = 5f;
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

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound2D("PickUpItem");

        hp.StartCoroutine(FreezeRoutine());

        Destroy(gameObject);
    }

    private IEnumerator FreezeRoutine()
    {
        // Kumpulkan semua enemy saat ini
        EnemyAI[]     ais     = Object.FindObjectsOfType<EnemyAI>(true);
        EnemyFSM[]    fsms    = Object.FindObjectsOfType<EnemyFSM>(true);
        EnemyShoot[]  shoots  = Object.FindObjectsOfType<EnemyShoot>(true);
        Rigidbody2D[] rbs     = new Rigidbody2D[ais.Length];

        // Beku: disable komponen AI & stop fisika
        for (int i = 0; i < ais.Length; i++)
        {
            if (ais[i] == null) continue;
            rbs[i] = ais[i].GetComponent<Rigidbody2D>();
            if (rbs[i] != null)
            {
                rbs[i].velocity        = Vector2.zero;
                rbs[i].angularVelocity = 0f;
                rbs[i].isKinematic     = true; // hentikan fisika
            }
            ais[i].enabled = false;
        }

        foreach (var fsm in fsms)
            if (fsm != null) fsm.enabled = false;

        foreach (var shoot in shoots)
            if (shoot != null) shoot.enabled = false;

        yield return new WaitForSeconds(frezeTime);

        // Cairkan: enable kembali semua komponen
        for (int i = 0; i < ais.Length; i++)
        {
            if (ais[i] == null) continue;
            if (rbs[i] != null) rbs[i].isKinematic = false;
            ais[i].enabled = true;
        }

        foreach (var fsm in fsms)
            if (fsm != null) fsm.enabled = true;

        foreach (var shoot in shoots)
            if (shoot != null) shoot.enabled = true;
    }
}
