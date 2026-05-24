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
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

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
    }

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