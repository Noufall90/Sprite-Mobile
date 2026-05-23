using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private int currentWave = 0;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private float resultShowDelay = 1.5f;
    [SerializeField] private float prepDuration;

    [Header("References")]
    [SerializeField] private WaveSpawn waveSpawn;
    [SerializeField] private WaveTime waveTime;
    [SerializeField] private WaveUI waveUI;
    [SerializeField] private TransitionManager transitionManager;

    private int waveScore = 0;
    private int highestScore = 0;
    private bool waveHasEnded = false;

    public static Action<int> OnWaveStart;
    public static Action OnWaveEnd;

    private void Awake()
    {
        highestScore = (HighScoreManager.Instance != null) ? HighScoreManager.Instance.HighestScore : PlayerPrefs.GetInt("HighestScore", 0);

        if (waveSpawn == null) waveSpawn = FindObjectOfType<WaveSpawn>();
        if (waveTime == null) waveTime = FindObjectOfType<WaveTime>();
        if (waveUI == null) waveUI = FindObjectOfType<WaveUI>();
        if (transitionManager == null) transitionManager = FindObjectOfType<TransitionManager>();
    }

    private void Start()
    {
        Time.timeScale = 1f;
        StartWave();
    }

    private void Update()
    {
        if (!waveHasEnded && waveTime != null && waveTime.IsGameplayTimeUp())
        {
            EndWave();
        }
    }

    public void StartWave()
    {
        waveHasEnded = false;
        waveScore = 0;

        if (playerSpawnPoint != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                player.transform.position = playerSpawnPoint.position;
            }
        }

        StartCoroutine(WaveStartSequence());
    }

    private IEnumerator WaveStartSequence()
    {
        // Preparation countdown
        if (waveTime != null)
        {
            waveTime.StartPrep(prepDuration);
        }

        while (waveTime != null && !waveTime.IsPrepFinished())
        {
            yield return null;
        }

        // Activate enemy spawns
        if (waveSpawn != null)
        {
            waveSpawn.ActivateWave(currentWave);
        }

        // Start gameplay timer
        if (waveTime != null)
        {
            waveTime.StartGameplayTimer(currentWave);
        }

        OnWaveStart?.Invoke(currentWave);
        currentWave++;
    }

    public void EndWave()
    {
        if (waveHasEnded) return;

        waveHasEnded = true;

        // Update global high score via HighScoreManager
        if (HighScoreManager.Instance != null)
        {
            HighScoreManager.Instance.TrySetNewHighScore(waveScore);
            highestScore = HighScoreManager.Instance.HighestScore;
        }
        else if (waveScore > highestScore)
        {
            highestScore = waveScore;
            PlayerPrefs.SetInt("HighestScore", highestScore);
            PlayerPrefs.Save();
        }

        ClearRemainingEnemies();

        OnWaveEnd?.Invoke();

        StartCoroutine(ShowResultsAfterDelay());
    }

    private IEnumerator ShowResultsAfterDelay()
    {
        yield return new WaitForSeconds(resultShowDelay);

        Time.timeScale = 0f;

        if (waveUI != null)
        {
            waveUI.ShowResults(currentWave - 1, waveScore, highestScore);
        }
    }

    private void ClearRemainingEnemies()
    {
        EnemyHealth[] enemies = FindObjectsOfType<EnemyHealth>();

        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy == null) continue;

            Destroy(enemy.transform.root.gameObject);
        }
    }

    public void NextWave()
    {
        Time.timeScale = 1f;
        StartWave();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("MainMenu");
    }

    public void AddWaveScore(int points)
    {
        waveScore += points;
    }
}