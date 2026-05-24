using System.Collections;
using UnityEngine;

/// <summary>
/// sizeItem — ketika player mengambil item ini:
/// - Scale peluru (bulletPrefab yang diinstantiate) diubah dari default menjadi (0.5, 0.5, 0.5)
/// - Setelah sizeTime detik, dikembalikan ke scale semula (0.2, 0.2, 0.2)
/// Cara kerja: mengubah localScale pada bulletPrefab via Shoot component
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class sizeItem : MonoBehaviour
{
    [Header("Size Settings")]
    [SerializeField] private float sizeTime = 5f;
    [SerializeField] private Vector3 boostedScale  = new Vector3(0.5f, 0.5f, 0.5f);
    [SerializeField] private Vector3 originalScale = new Vector3(0.2f, 0.2f, 0.2f);
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

        Shoot shoot = other.GetComponentInParent<Shoot>();
        if (shoot == null) shoot = other.GetComponent<Shoot>();
        if (shoot == null) shoot = other.GetComponentInChildren<Shoot>();
        if (shoot == null) return;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound2D("PickUpItem");

        hp.StartCoroutine(SizeBoostRoutine(shoot));

        Destroy(gameObject);
    }

    private IEnumerator SizeBoostRoutine(Shoot shoot)
    {
        // Ubah scale bullet prefab
        if (shoot.bulletPrefab != null)
            shoot.bulletPrefab.transform.localScale = boostedScale;

        yield return new WaitForSeconds(sizeTime);

        // Kembalikan ke semula
        if (shoot != null && shoot.bulletPrefab != null)
            shoot.bulletPrefab.transform.localScale = originalScale;
    }
}
