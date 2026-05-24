using UnityEngine;
using TMPro;

public class WaveUI : MonoBehaviour
{
    public static WaveUI Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TMP_Text waveNumberText;
    [SerializeField] private TMP_Text waveScoreText;
    [SerializeField] private TMP_Text highestScoreText;
    [SerializeField] private TMP_Text currentWave;

    public TMP_Text waveScroreGame; // live score display during gameplay

    private WaveManager waveManager;
    private int currentScore = 0;
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

        if (waveScoreText != null)
        {
            waveScoreText.text = $"Wave Score: {waveScore}";
        }

        if (highestScoreText != null)
        {
            highestScoreText.text = $"Highest Score: {highestScore}";
        }

        // Synchronize with HighScoreManager
        if (HighScoreManager.Instance != null)
        {
            HighScoreManager.Instance.TrySetNewHighScore(highestScore);

            if (highestScoreText != null)
            {
                highestScoreText.text =
                    $"Highest Score: {HighScoreManager.Instance.HighestScore}";
            }
        }
    }

    private void OnWaveStart(int waveNumber)
    {
        currentScore = 0;

        if (currentWave != null)
        {
            // waveNumber dimulai dari 0, tampilkan +1 untuk UI (Wave 1, 2, 3...)
            string maxStr = maxWave > 0 ? maxWave.ToString() : "?";
            currentWave.text = $"Wave {waveNumber + 1} / {maxStr}";
        }

        UpdateScoreUI();
    }

    private void OnEnemyKilled(int points)
    {
        currentScore += points;

        UpdateScoreUI();

        if (waveManager != null)
        {
            waveManager.AddWaveScore(points);
        }
    }

    private void UpdateScoreUI()
    {
        if (waveScroreGame != null)
        {
            waveScroreGame.text = currentScore.ToString();
        }
    }

    public bool IsResultsVisible()
    {
        return resultsPanel != null && resultsPanel.activeSelf;
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