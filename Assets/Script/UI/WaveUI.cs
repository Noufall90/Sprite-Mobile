using UnityEngine;
using TMPro;

public class WaveUI : MonoBehaviour
{
    public static WaveUI Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TMP_Text waveNumberText;
    [SerializeField] private TMP_Text waveScoreText;       // Skor wave saat ini
    [SerializeField] private TMP_Text totalLevelScoreText; // Skor akumulasi seluruh level
    [SerializeField] private TMP_Text highestScoreText;
    [SerializeField] private TMP_Text currentWave;

    [Header("Death Pop-up")]
    [SerializeField] private TMP_Text highestScoreTextDeath;

    public TMP_Text waveScroreGame; // Live total level score display saat gameplay

    private WaveManager waveManager;
    private int currentScore = 0;      // Live total level score (tidak reset per wave)
    private int currentWaveScore = 0;  // Skor wave saat ini saja
    private int maxWave = 0;

    private void Start()
    {
        Instance = this;

        waveManager = FindObjectOfType<WaveManager>();

        // Ambil maxWave dari WaveManager untuk ditampilkan di UI
        if (waveManager != null)
            maxWave = waveManager.MaxWave;

        if (resultsPanel != null)
        {
            resultsPanel.SetActive(false);
        }

        // Subscribe to events to track live scoring
        EnemyHealth.OnEnemyKilled += OnEnemyKilled;
        WaveManager.OnWaveStart += OnWaveStart;
    }

    private void OnDestroy()
    {
        EnemyHealth.OnEnemyKilled -= OnEnemyKilled;
        WaveManager.OnWaveStart -= OnWaveStart;
    }

    public void ShowResults(
        int waveNumber,
        int waveScore,
        int totalLevelScore,
        int highestScore
    )
    {
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(true);
        }

        WaveTime waveTime = FindObjectOfType<WaveTime>();

        if (waveTime != null)
        {
            waveTime.SetTimeZero();
        }

        if (waveNumberText != null)
        {
            waveNumberText.text = $"Wave {waveNumber}";
        }

        // Skor wave ini
        if (waveScoreText != null)
        {
            waveScoreText.text = $"Wave Score: {waveScore}";
        }

        // Total skor seluruh level
        if (totalLevelScoreText != null)
        {
            totalLevelScoreText.text = $"Level Score: {totalLevelScore}";
        }

        if (highestScoreText != null)
        {
            highestScoreText.text = $"Highest Score: {highestScore}";
        }

        // Sinkronisasi dengan HighScoreManager (gunakan totalLevelScore)
        if (HighScoreManager.Instance != null)
        {
            HighScoreManager.Instance.TrySetNewHighScore(totalLevelScore);

            if (highestScoreText != null)
            {
                highestScoreText.text =
                    $"Highest Score: {HighScoreManager.Instance.HighestScore}";
            }
        }
    }

    private void OnWaveStart(int waveNumber)
    {
        // currentScore (total level) TIDAK direset — terus akumulasi
        currentWaveScore = 0;

        if (currentWave != null)
        {
            string maxStr = maxWave > 0 ? maxWave.ToString() : "?";
            currentWave.text = $"Wave {waveNumber + 1} / {maxStr}";
        }

        UpdateScoreUI();
    }

    private void OnEnemyKilled(int points)
    {
        currentScore      += points; // Total level
        currentWaveScore  += points; // Wave ini saja

        UpdateScoreUI();

        if (waveManager != null)
        {
            waveManager.AddWaveScore(points);
        }
    }

    private void UpdateScoreUI()
    {
        // waveScroreGame menampilkan total skor level (terakumulasi)
        if (waveScroreGame != null)
        {
            waveScroreGame.text = currentScore.ToString();
        }
    }

    public bool IsResultsVisible()
    {
        return resultsPanel != null && resultsPanel.activeSelf;
    }

    /// <summary>
    /// Dipanggil oleh HealthPlayer saat player mati.
    /// Menampilkan highest score pada death pop-up.
    /// </summary>
    public void ShowDeathScore()
    {
        if (highestScoreTextDeath == null) return;

        int hs = HighScoreManager.Instance != null
            ? HighScoreManager.Instance.HighestScore
            : PlayerPrefs.GetInt("HighestScore", 0);

        highestScoreTextDeath.text = $"Highest Score: {hs}";
    }

    // Assign dari Button OnClick Inspector
    public void NextWave()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D("Button");
        }

        if (resultsPanel != null)
        {
            resultsPanel.SetActive(false);
        }

        if (waveManager != null)
        {
            waveManager.NextWave();
        }
    }

    // Assign dari Button OnClick Inspector
    public void MainMenu()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D("Button");
        }

        if (waveManager != null)
        {
            waveManager.GoToMainMenu();
        }
    }
}