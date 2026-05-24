using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private int currentWave = 0;
    [SerializeField] private int maxWave = 0;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private float resultShowDelay = 1.5f;
    [SerializeField] private float prepDuration;

    [Header("References")]
    [SerializeField] private WaveSpawn waveSpawn;
    [SerializeField] private WaveTime waveTime;
    [SerializeField] private WaveUI waveUI;
    [SerializeField] private LevelUI levelUI;

    // Property publik untuk dibaca WaveUI dan LevelUI
    public int MaxWave => maxWave;
    public int CurrentWave => currentWave;

    private int waveScore = 0;
    private int highestScore = 0;
    private bool waveHasEnded = false;

    public static Action<int> OnWaveStart;
    public static Action OnWaveEnd;

    private void Awake()
    {
        // Get highest score from HighScoreManager (persistent singleton)
        if (HighScoreManager.Instance != null)
        {
            highestScore = HighScoreManager.Instance.HighestScore;
        }
        else
        {
            highestScore = PlayerPrefs.GetInt("HighestScore", 0);
        }

        if (waveSpawn == null) waveSpawn = FindObjectOfType<WaveSpawn>();
        if (waveTime == null) waveTime = FindObjectOfType<WaveTime>();
        if (waveUI == null) waveUI = FindObjectOfType<WaveUI>();

        // === DEBUG: Cek semua referensi ===
        Debug.Log($"[WaveManager] Awake — waveSpawn: {(waveSpawn != null ? waveSpawn.name : "NULL")}");
        Debug.Log($"[WaveManager] Awake — waveTime:  {(waveTime != null ? waveTime.name : "NULL")}");
        Debug.Log($"[WaveManager] Awake — waveUI:    {(waveUI != null ? waveUI.name : "NULL")}");

        if (waveSpawn == null)
            Debug.LogError("[WaveManager] waveSpawn TIDAK DITEMUKAN! Assign di Inspector atau pastikan ada WaveSpawn di scene.");
        if (waveTime == null)
            Debug.LogError("[WaveManager] waveTime TIDAK DITEMUKAN! Assign di Inspector atau pastikan ada WaveTime di scene.");
    }

    private void Start()
    {
        Debug.Log("[WaveManager] Start dipanggil — memulai wave pertama.");
        Time.timeScale = 1f;
        StartWave();
    }

    private void Update()
    {
        if (!waveHasEnded && waveTime != null && waveTime.IsGameplayTimeUp())
        {
            EndWave();
        }
    }

    public void StartWave()
    {
        waveHasEnded = false;
        waveScore = 0;

        Debug.Log($"[WaveManager] StartWave() — wave ke-{currentWave + 1} dimulai.");

        if (playerSpawnPoint != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                player.transform.position = playerSpawnPoint.position;
                Debug.Log($"[WaveManager] Player direset ke spawn point: {playerSpawnPoint.position}");
            }
            else
            {
                Debug.LogWarning("[WaveManager] Tidak menemukan GameObject dengan tag 'Player'!");
            }
        }

        StartCoroutine(WaveStartSequence());
    }

    private IEnumerator WaveStartSequence()
    {
        Debug.Log($"[WaveManager] WaveStartSequence mulai — prepDuration: {prepDuration}");

        // Tunggu 1 frame agar semua Start() komponen lain selesai
        yield return null;

        // Preparation countdown
        if (waveTime != null)
        {
            Debug.Log($"[WaveManager] Memulai prep timer: {prepDuration} detik.");
            waveTime.StartPrep(prepDuration);
        }
        else
        {
            Debug.LogWarning("[WaveManager] waveTime null — prep timer dilewati.");
        }

        // Tunggu prep selesai — jika prepDuration = 0, langsung lanjut
        if (waveTime != null && prepDuration > 0f)
        {
            Debug.Log("[WaveManager] Menunggu prep selesai...");
            while (!waveTime.IsPrepFinished())
            {
                yield return null;
            }
            Debug.Log("[WaveManager] Prep selesai!");
        }

        // Activate enemy spawns
        if (waveSpawn != null)
        {
            Debug.Log($"[WaveManager] Mengaktifkan WaveSpawn untuk wave index: {currentWave}");
            waveSpawn.ActivateWave(currentWave);
        }
        else
        {
            Debug.LogError("[WaveManager] waveSpawn NULL — enemy TIDAK akan spawn! Assign WaveSpawn di Inspector.");
        }

        // Start gameplay timer
        if (waveTime != null)
        {
            Debug.Log($"[WaveManager] Memulai gameplay timer untuk wave: {currentWave}");
            waveTime.StartGameplayTimer(currentWave);
        }
        else
        {
            Debug.LogError("[WaveManager] waveTime NULL — gameplay timer tidak berjalan! Wave tidak akan pernah berakhir.");
        }

        Debug.Log($"[WaveManager] OnWaveStart dipanggil untuk wave: {currentWave}");
        OnWaveStart?.Invoke(currentWave);
        currentWave++;

        Debug.Log($"[WaveManager] WaveStartSequence selesai. currentWave sekarang: {currentWave}");
    }

    public void EndWave()
    {
        if (waveHasEnded) return;

        waveHasEnded = true;

        // Update highest score via HighScoreManager
        if (HighScoreManager.Instance != null)
        {
            HighScoreManager.Instance.TrySetNewHighScore(waveScore);
            highestScore = HighScoreManager.Instance.HighestScore;
        }
        else if (waveScore > highestScore)
        {
            highestScore = waveScore;
            PlayerPrefs.SetInt("HighestScore", highestScore);
            PlayerPrefs.Save();
        }

        ClearRemainingEnemies();

        OnWaveEnd?.Invoke();

        StartCoroutine(ShowResultsAfterDelay());
    }

    private IEnumerator ShowResultsAfterDelay()
    {
        yield return new WaitForSeconds(resultShowDelay);

        Time.timeScale = 0f;

        bool allWavesDone = maxWave > 0 && currentWave >= maxWave;

        if (allWavesDone)
        {
            // Semua wave selesai — tampilkan Level Complete UI
            if (levelUI != null)
            {
                levelUI.ShowLevelResults(waveScore, highestScore);
            }
            else if (waveUI != null)
            {
                // Fallback jika LevelUI belum di-assign
                waveUI.ShowResults(currentWave - 1, waveScore, highestScore);
            }
        }
        else
        {
            // Masih ada wave berikutnya — tampilkan hasil wave biasa
            if (waveUI != null)
            {
                waveUI.ShowResults(currentWave - 1, waveScore, highestScore);
            }
        }
    }

    private void ClearRemainingEnemies()
    {
        EnemyHealth[] enemies = FindObjectsOfType<EnemyHealth>();

        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy == null) continue;

            Destroy(enemy.transform.root.gameObject);
        }
    }

    public void NextWave()
    {
        Time.timeScale = 1f;
        StartWave();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        if (TransitionManager.Instance != null)
            TransitionManager.Instance.LoadScene(SceneManager.GetActiveScene().name);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;

        if (TransitionManager.Instance != null)
            TransitionManager.Instance.LoadScene("MainMenu");
        else
            SceneManager.LoadScene("MainMenu");
    }

    public void AddWaveScore(int points)
    {
        waveScore += points;
    }
}