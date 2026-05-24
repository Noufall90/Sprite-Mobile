using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// LevelUI — ditampilkan ketika semua wave (maxWave) sudah selesai.
/// Menampilkan hasil score dan menyediakan tombol NextLevel / MainMenu.
///
/// Setup di Inspector:
/// 1. Assign 'levelPanel' ke Panel/Canvas yang menampilkan hasil level
/// 2. Assign text fields untuk score
/// 3. Set 'nextSceneName' ke nama scene level berikutnya
/// 4. Attach tombol NextLevel dan MainMenu ke method di sini
/// </summary>
public class LevelUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject levelPanel;

    [Header("Score Texts")]
    [SerializeField] private TMP_Text scoreTitleText;   // e.g. "Level Complete!"
    [SerializeField] private TMP_Text waveScoreText;    // Score wave ini
    [SerializeField] private TMP_Text highestScoreText; // Highest score

    [Header("Navigation")]
    [Tooltip("Nama scene level berikutnya. Kosongkan jika tidak ada.")]
    [SerializeField] private string nextSceneName = "";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private WaveManager waveManager;

    private void Start()
    {
        waveManager = FindObjectOfType<WaveManager>();

        // Pastikan panel tersembunyi di awal
        if (levelPanel != null)
            levelPanel.SetActive(false);
    }

    // ── Dipanggil oleh WaveManager saat semua wave selesai ───────

    public void ShowLevelResults(int waveScore, int highestScore)
    {
        // Aktifkan panel
        if (levelPanel != null)
            levelPanel.SetActive(true);

        // Tampilkan score
        if (waveScoreText != null)
            waveScoreText.text = $"Score: {waveScore}";

        if (highestScoreText != null)
            highestScoreText.text = $"Highest: {highestScore}";

        // Sync dengan HighScoreManager
        if (HighScoreManager.Instance != null)
        {
            HighScoreManager.Instance.TrySetNewHighScore(waveScore);
            if (highestScoreText != null)
                highestScoreText.text = $"Highest: {HighScoreManager.Instance.HighestScore}";
        }
    }

    // ── Tombol dari Inspector ─────────────────────────────────────

    /// <summary>Pindah ke level berikutnya.</summary>
    public void NextLevel()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound2D("Button");

        Time.timeScale = 1f;

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("[LevelUI] nextSceneName belum diisi! Kembali ke MainMenu.");
            SceneManager.LoadScene(mainMenuSceneName);
            return;
        }

        if (TransitionManager.Instance != null)
            TransitionManager.Instance.LoadScene(nextSceneName);
        else
            SceneManager.LoadScene(nextSceneName);
    }

    /// <summary>Kembali ke Main Menu.</summary>
    public void MainMenu()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound2D("Button");

        Time.timeScale = 1f;

        if (TransitionManager.Instance != null)
            TransitionManager.Instance.LoadScene(mainMenuSceneName);
        else
            SceneManager.LoadScene(mainMenuSceneName);
    }
}
