using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WaveUI : MonoBehaviour
{
    public static WaveUI Instance { get; private set; }
    [Header("UI Elements")]
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TMP_Text waveNumberText;
    [SerializeField] private TMP_Text waveScoreText;
    [SerializeField] private TMP_Text highestScoreText;
    [SerializeField] private Button nextWaveButton;
    [SerializeField] private Button mainMenuButton;
    public TMP_Text waveScroreGame; // live score display during gameplay

    private WaveManager waveManager;
    private int currentScore = 0;

    private void Start()
    {
        Instance = this;
        waveManager = FindObjectOfType<WaveManager>();

        if (resultsPanel != null)
        {
            resultsPanel.SetActive(false);
        }

        if (nextWaveButton != null)
        {
            nextWaveButton.onClick.AddListener(OnNextWaveClicked);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
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

        WaveTime waveTime =
            FindObjectOfType<WaveTime>();

        if (waveTime != null)
        {
            waveTime.SetTimeZero();
        }

        if (waveNumberText != null)
        {
            waveNumberText.text =
                $"Wave {waveNumber}";
        }

        if (waveScoreText != null)
        {
            waveScoreText.text =
                $"Wave Score: {waveScore}";
        }

        if (highestScoreText != null)
        {
            highestScoreText.text =
                $"Highest Score: {highestScore}";
        }

        // Synchronize with HighScoreManager (source of truth)
        if (HighScoreManager.Instance != null)
        {
            HighScoreManager.Instance.TrySetNewHighScore(highestScore);
            // Ensure displayed value reflects the manager's stored value
            if (highestScoreText != null)
            {
                highestScoreText.text = $"Highest Score: {HighScoreManager.Instance.HighestScore}";
            }
        }
    }

    private void OnWaveStart(int waveNumber)
    {
        currentScore = 0;
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

    public void OnNextWaveClicked()
    {
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(false);
        }

        if (waveManager != null)
        {
            waveManager.NextWave();
        }
    }

    public void OnMainMenuClicked()
    {
        if (waveManager != null)
        {
            waveManager.GoToMainMenu();
        }
    }
}