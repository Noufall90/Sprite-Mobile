using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WaveUI : MonoBehaviour
{
    public static WaveUI Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject resultsPanel;

    [Header("Result Panel")]
    [SerializeField] private TMP_Text waveNumberText;
    [SerializeField] private TMP_Text waveScoreText;
    [SerializeField] private TMP_Text highestScoreText;

    [Header("Gameplay HUD")]
    [SerializeField] private TMP_Text waveScoreGameText;

    [Header("Buttons")]
    [SerializeField] private Button nextWaveButton;
    [SerializeField] private Button mainMenuButton;

    private WaveManager waveManager;

    // Total score selama game
    private int currentScore = 0;

    // Score khusus wave sekarang
    private int waveScore = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        waveManager = FindObjectOfType<WaveManager>();

        EnemyHealth.OnEnemyKilled += OnEnemyKilled;
        WaveManager.OnWaveStart += OnWaveStart;

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

        UpdateScoreUI();
    }

    private void OnDestroy()
    {
        EnemyHealth.OnEnemyKilled -= OnEnemyKilled;
        WaveManager.OnWaveStart -= OnWaveStart;
    }

    public void ShowResults(
        int waveNumber,
        int waveScoreResult,
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

        // Score wave saja
        if (waveScoreText != null)
        {
            waveScoreText.text = $"Wave Score: {waveScoreResult}";
        }

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
        // Reset hanya score wave
        waveScore = 0;

        UpdateScoreUI();
    }

    private void OnEnemyKilled(int points)
    {
        // Tambah total score
        currentScore += points;

        // Tambah score wave
        waveScore += points;

        UpdateScoreUI();

        if (waveManager != null)
        {
            waveManager.AddWaveScore(points);
        }

        // Update highscore realtime
        if (HighScoreManager.Instance != null)
        {
            HighScoreManager.Instance.TrySetNewHighScore(currentScore);
        }
    }

    private void UpdateScoreUI()
    {
        // HUD realtime total score
        if (waveScoreGameText != null)
        {
            waveScoreGameText.text = $"Score : {currentScore}";
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