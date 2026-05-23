using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string playSceneName;

    [Header("UI")]
    [SerializeField] private TMP_Text highestScoreText;

    private void Start()
    {
        // Register highest score text
        if (highestScoreText != null && HighScoreManager.Instance != null)
        {
            HighScoreManager.Instance.SetMainMenuText(highestScoreText);
        }

        // Play main menu music
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMusic("MainMenu");
        }
    }

    public void PlayGame()
    {
        if (string.IsNullOrEmpty(playSceneName))
        {
            Debug.LogWarning("Scene name belum diisi.");
            return;
        }

        // Play game music
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMusic("Game");
        }

        SceneManager.LoadScene(playSceneName);
    }

    public void ExitGame()
    {
        Debug.Log("Keluar dari game...");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}