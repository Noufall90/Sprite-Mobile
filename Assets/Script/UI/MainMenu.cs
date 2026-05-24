using UnityEngine;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text highestScoreText;

    [Header("Guide")]
    [SerializeField] private GameObject guideCanvas;

    [Header("Level Menu")]
    [SerializeField] private GameObject levelMenuCanvas;

    private void Start()
    {
        if (highestScoreText != null && HighScoreManager.Instance != null)
        {
            HighScoreManager.Instance.SetMainMenuText(highestScoreText);
        }

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMusic("MainMenu");
        }

        // Pastikan level menu tertutup saat awal
        if (levelMenuCanvas != null)
        {
            levelMenuCanvas.SetActive(false);
        }
    }

    // Tombol PLAY -> buka level menu
    public void OpenLevel()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound2D("Button");

        if (levelMenuCanvas != null)
        {
            levelMenuCanvas.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Level Menu Canvas belum di-assign.");
        }
    }

    // Tombol CLOSE level menu
    public void CloseLevel()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound2D("Button");

        if (levelMenuCanvas != null)
        {
            levelMenuCanvas.SetActive(false);
        }
    }

    public void Guide()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound2D("Button");

        if (guideCanvas != null)
            guideCanvas.SetActive(true);
        else
            Debug.LogWarning("Guide Canvas belum di-assign.");
    }

    public void CloseGuide()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound2D("Button");

        if (guideCanvas != null)
            guideCanvas.SetActive(false);
    }

    public void ExitGame()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound2D("Button");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}