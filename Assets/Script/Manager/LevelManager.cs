using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private Button[] buttons;

    private void Awake()
    {
        int unlockedLevel = PlayerPrefs.GetInt(GameManagerPlayer.KEY_UNLOCKED_LEVEL, 1);

        // Jika player pernah mati di suatu scene, pastikan level itu masuk hitungan terbuka
        string lastScene = PlayerPrefs.GetString(GameManagerPlayer.KEY_LAST_SCENE, "");
        if (!string.IsNullOrEmpty(lastScene) && SceneLevelIndex.TryGetValue(lastScene, out int lastIndex))
        {
            int minRequired = lastIndex + 1;
            if (unlockedLevel < minRequired)
            {
                unlockedLevel = minRequired;
                // Sinkronkan kembali ke PlayerPrefs
                PlayerPrefs.SetInt(GameManagerPlayer.KEY_UNLOCKED_LEVEL, unlockedLevel);
                PlayerPrefs.Save();
            }
        }

        // Disable semua tombol
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = false;
        }

        // Enable level yang sudah terbuka
        for (int i = 0; i < unlockedLevel; i++)
        {
            if (i < buttons.Length)
            {
                buttons[i].interactable = true;
            }
        }

        Debug.Log($"[LevelManager] UnlockedLevel={unlockedLevel}, LastPlayedScene='{lastScene}'");
    }

    // Mapping nama scene → indeks tombol (harus sinkron dengan GameManagerPlayer)
    private static readonly System.Collections.Generic.Dictionary<string, int> SceneLevelIndex =
        new System.Collections.Generic.Dictionary<string, int>
        {
            { "Level_1", 0 },
            { "Level_2", 1 },
            { "Level_3", 2 },
            // Tambahkan scene lain di sini jika ada
        };

    public void OpenLevel(string levelName)
    {
        // Play button sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D("Button");
        }

        // Cek apakah scene ada di Build Settings
        if (!Application.CanStreamedLevelBeLoaded(levelName))
        {
            Debug.LogError("Scene tidak ditemukan di Build Settings: " + levelName);
            return;
        }

        // Gunakan TransitionManager untuk fade out sebelum load scene
        if (TransitionManager.Instance != null)
        {
            Debug.Log($"[LevelManager] Membuka scene '{levelName}' via TransitionManager.");
            TransitionManager.Instance.LoadScene(levelName);
        }
        else
        {
            Debug.LogWarning("[LevelManager] TransitionManager tidak ditemukan, load scene langsung.");
            SceneManager.LoadScene(levelName);
        }
    }
}