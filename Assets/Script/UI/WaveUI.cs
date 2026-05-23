using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WaveUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TMP_Text waveNumberText;
    [SerializeField] private TMP_Text waveScoreText;
    [SerializeField] private TMP_Text highestScoreText;
    [SerializeField] private Button nextWaveButton;
    [SerializeField] private Button mainMenuButton;

    private WaveManager waveManager;

    private void Start()
    {
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

        // Persist highest score to PlayerPrefs so MainMenu reads the same value
        PlayerPrefs.SetInt("HighestScore", highestScore);
        PlayerPrefs.Save();
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