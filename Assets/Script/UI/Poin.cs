using UnityEngine;
using TMPro;

public class Poin : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;

    [Header("Settings")]
    [SerializeField] private int startingScore = 0;

    private int score;
    private WaveManager waveManager;

    private void Awake()
    {
        score = startingScore;
        waveManager = FindObjectOfType<WaveManager>();
        UpdateUI();
    }

    private void OnEnable()
    {
        EnemyHealth.OnEnemyKilled += OnEnemyKilled;
        if (WaveManager.OnWaveStart != null)
            WaveManager.OnWaveStart += OnWaveStart;
    }

    private void OnDisable()
    {
        EnemyHealth.OnEnemyKilled -= OnEnemyKilled;
        if (WaveManager.OnWaveStart != null)
            WaveManager.OnWaveStart -= OnWaveStart;
    }

    private void OnWaveStart(int waveNumber)
    {
        score = startingScore;
        UpdateUI();
    }

    private void OnEnemyKilled(int points)
    {
        AddPoints(points);
    }

    public void AddPoints(int points)
    {
        score += points;
        UpdateUI();

        // Notify WaveManager of new wave score
        if (waveManager != null)
        {
            waveManager.AddWaveScore(points);
        }
    }

    private void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }
}
