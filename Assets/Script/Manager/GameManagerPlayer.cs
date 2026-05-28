using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Mengelola state player per-level.
/// Dipasang di setiap level scene (bukan Main Menu).
/// Menyimpan progress unlock level ke PlayerPrefs agar
/// LevelManager di Main Menu bisa membuka tombol yang sesuai.
/// </summary>
public class GameManagerPlayer : MonoBehaviour
{
    // ─── Singleton (per-scene, bukan DontDestroyOnLoad) ───────────────────────
    public static GameManagerPlayer Instance { get; private set; }

    // ─── PlayerPrefs Keys (harus sama dengan yang digunakan LevelManager) ─────
    public const string KEY_UNLOCKED_LEVEL = "UnlockedLevel";
    public const string KEY_LAST_SCENE     = "LastPlayedScene";

    // ─── Scene Index Map ───────────────────────────────────────────────────────
    // Mapping nama scene → indeks tombol di LevelManager (0-based).
    // Tambahkan entri baru jika ada scene level tambahan.
    private static readonly Dictionary<string, int> SceneLevelIndex = new Dictionary<string, int>
    {
        { "Level_1", 0 },
        { "Level_2", 1 },
        { "Level_3", 2 },
        // Tambahkan scene lain di sini, misalnya: { "Level_4", 3 }
    };

    // ──────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        // Singleton per-scene (tidak perlu DontDestroyOnLoad)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Catat scene ini sebagai scene yang sedang dimainkan
        string currentScene = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString(KEY_LAST_SCENE, currentScene);
        PlayerPrefs.Save();

        Debug.Log($"[GameManagerPlayer] Scene aktif: '{currentScene}' disimpan ke PlayerPrefs.");
    }

    /// <summary>
    /// Dipanggil oleh HealthPlayer saat player mati.
    /// Menyimpan scene saat ini ke PlayerPrefs agar tombol level
    /// tetap terbuka ketika kembali ke Main Menu.
    /// </summary>
    public void OnPlayerDied()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Simpan scene terakhir yang dimainkan
        PlayerPrefs.SetString(KEY_LAST_SCENE, currentScene);

        // Pastikan level ini (dan semua sebelumnya) tetap terbuka
        if (SceneLevelIndex.TryGetValue(currentScene, out int levelIndex))
        {
            // UnlockedLevel menyimpan jumlah level yang terbuka (1-based count).
            // Minimal kita pastikan level ini sendiri terbuka (index + 1).
            int currentUnlocked = PlayerPrefs.GetInt(KEY_UNLOCKED_LEVEL, 1);
            int minRequired = levelIndex + 1; // level ini harus terbuka

            if (currentUnlocked < minRequired)
            {
                PlayerPrefs.SetInt(KEY_UNLOCKED_LEVEL, minRequired);
                Debug.Log($"[GameManagerPlayer] UnlockedLevel diperbarui ke {minRequired}.");
            }
        }
        else
        {
            Debug.LogWarning($"[GameManagerPlayer] Scene '{currentScene}' tidak ada di SceneLevelIndex. " +
                             "Tambahkan mapping-nya agar tombol level terbuka dengan benar.");
        }

        PlayerPrefs.Save();
        Debug.Log($"[GameManagerPlayer] Player mati di '{currentScene}'. Progress disimpan.");
    }

    /// <summary>
    /// Dipanggil saat player berhasil menyelesaikan level.
    /// Membuka level BERIKUTNYA di Main Menu.
    /// </summary>
    public void OnLevelCompleted()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (SceneLevelIndex.TryGetValue(currentScene, out int levelIndex))
        {
            // Buka level berikutnya (index + 2 karena UnlockedLevel adalah count 1-based)
            int nextUnlock = levelIndex + 2;
            int currentUnlocked = PlayerPrefs.GetInt(KEY_UNLOCKED_LEVEL, 1);

            if (nextUnlock > currentUnlocked)
            {
                PlayerPrefs.SetInt(KEY_UNLOCKED_LEVEL, nextUnlock);
                Debug.Log($"[GameManagerPlayer] Level selesai! UnlockedLevel diperbarui ke {nextUnlock}.");
            }
        }

        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
