// WaveSpawn.cs
using UnityEngine;

public class WaveSpawn : MonoBehaviour
{
    [Header("Spawn Objects")]
    [SerializeField] private GameObject[] spawnPoints;

    [SerializeField] private bool activateSequentially = true;

    private void Start()
    {
        DeactivateAllSpawns();
    }

    public void ActivateWave(int waveIndex)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("WaveSpawn: Spawn point kosong!");
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

            Debug.Log($"Wave {waveIndex}: spawn aktif 0 s/d {upTo}");
        }
        else
        {
            foreach (GameObject sp in spawnPoints)
            {
                if (sp != null) sp.SetActive(true);
            }
        }
    }

    public void DeactivateAllSpawns()
    {
        foreach (GameObject sp in spawnPoints)
        {
            if (sp != null) sp.SetActive(false);
        }
    }
}