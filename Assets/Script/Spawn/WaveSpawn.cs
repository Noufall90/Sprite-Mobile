// WaveSpawn.cs
using UnityEngine;

public class WaveSpawn : MonoBehaviour
{
    [Header("Spawn Objects")]
    [SerializeField] private GameObject[] spawnPoints;

    [SerializeField] private bool activateSequentially = true;

    // Gunakan Awake (bukan Start) agar tidak terjadi race condition
    // dengan WaveManager yang bisa mengaktifkan spawn via ActivateWave()
    // sebelum Start() dijalankan.
    private void Awake()
    {
        DeactivateAllSpawns();
        Debug.Log($"[WaveSpawn] Awake — semua spawn dimatikan. Total: {(spawnPoints != null ? spawnPoints.Length : 0)}");
    }

    public void ActivateWave(int waveIndex)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[WaveSpawn] spawnPoints kosong atau null! Assign di Inspector.");
            return;
        }

        // Matikan semua dulu
        foreach (GameObject sp in spawnPoints)
        {
            if (sp != null) sp.SetActive(false);
        }

        if (activateSequentially)
        {
            // Aktifkan hanya sampai index = waveIndex (tepat +1 per wave)
            int upTo = Mathf.Clamp(waveIndex, 0, spawnPoints.Length - 1);

            for (int i = 0; i <= upTo; i++)
            {
                if (spawnPoints[i] != null)
                    spawnPoints[i].SetActive(true);
            }

            Debug.Log($"[WaveSpawn] Wave {waveIndex}: spawn aktif 0 s/d {upTo} (dari {spawnPoints.Length} total)");
        }
        else
        {
            foreach (GameObject sp in spawnPoints)
            {
                if (sp != null) sp.SetActive(true);
            }

            Debug.Log($"[WaveSpawn] Wave {waveIndex}: semua {spawnPoints.Length} spawn diaktifkan.");
        }
    }

    public void DeactivateAllSpawns()
    {
        if (spawnPoints == null) return;
        foreach (GameObject sp in spawnPoints)
        {
            if (sp != null) sp.SetActive(false);
        }
    }
}