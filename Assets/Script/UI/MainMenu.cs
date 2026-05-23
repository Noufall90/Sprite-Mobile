using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string playSceneName;

    [Header("UI")]
    [SerializeField] private TMP_Text highestScoreText;

    [Header("Audio Settings")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        DisplayHighestScore();
        LoadVolume();

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMusic("MainMenu");
        }
    }

    private void DisplayHighestScore()
    {
        if (highestScoreText != null)
        {
            int highestScore = PlayerPrefs.GetInt("HighestScore", 0);

            highestScoreText.text = $"Highest Score: {highestScore}";

            Debug.Log($"Main Menu - Highest Score: {highestScore}");
        }
    }

    public void PlayGame()
    {
        if (string.IsNullOrEmpty(playSceneName))
        {
            Debug.LogWarning("Scene name belum diisi.");
            return;
        }

        SaveVolume();
        // Play music (if available) then load the game scene.
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMusic("Game");
        }

        SceneManager.LoadScene(playSceneName);
    }

    public void ExitGame()
    {
        Debug.Log("Keluar dari game...");

        SaveVolume();

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void UpdateMusicVolume(float volume)
    {
        if (audioMixer != null)
        {
            if (!audioMixer.SetFloat("MusicVolume", volume))
            {
                Debug.LogWarning("AudioMixer parameter 'MusicVolume' not found.");
            }
        }
    }

    public void UpdateSoundVolume(float volume)
    {
        if (audioMixer != null)
        {
            if (!audioMixer.SetFloat("SFXVolume", volume))
            {
                Debug.LogWarning("AudioMixer parameter 'SFXVolume' not found.");
            }
        }
    }

    public void SaveVolume()
    {
        if (audioMixer == null) return;
        float musicVolume = 0f;
        float sfxVolume = 0f;

        if (!audioMixer.GetFloat("MusicVolume", out musicVolume))
        {
            Debug.LogWarning("AudioMixer parameter 'MusicVolume' not found. Saving default 0.");
            musicVolume = 0f;
        }

        if (!audioMixer.GetFloat("SFXVolume", out sfxVolume))
        {
            Debug.LogWarning("AudioMixer parameter 'SFXVolume' not found. Saving default 0.");
            sfxVolume = 0f;
        }

        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);

        PlayerPrefs.Save();
    }

    public void LoadVolume()
    {
        if (musicSlider != null)
        {
            float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0f);

            musicSlider.value = musicVolume;

            if (audioMixer != null)
            {
                if (!audioMixer.SetFloat("MusicVolume", musicVolume))
                {
                    Debug.LogWarning("AudioMixer parameter 'MusicVolume' not found when loading saved value.");
                }
            }
        }

        if (sfxSlider != null)
        {
            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0f);

            sfxSlider.value = sfxVolume;

            if (audioMixer != null)
            {
                if (!audioMixer.SetFloat("SFXVolume", sfxVolume))
                {
                    Debug.LogWarning("AudioMixer parameter 'SFXVolume' not found when loading saved value.");
                }
            }
        }
    }
}