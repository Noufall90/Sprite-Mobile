using UnityEngine;
using TMPro;

public class WaveTime : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text timeText;

    [Header("Wave Settings")]
    public float timeWave;
    public float timeAddWave;
    private float timeRemaining = 0f;
    private bool isCountingDown = false;
    private bool isGameplayTimer = false;

    private void Start()
    {
        timeRemaining = 0f;
        isCountingDown = false;
        UpdateUI();
    }

    // Start the gameplay timer for the given wave number
    public void StartGameplayTimer(int waveNumber = 1)
    {
        float duration = timeWave + Mathf.Max(0, waveNumber - 1) * timeAddWave;
        SetTime(duration, true);
    }

    // Start a preparation / countdown timer before the wave begins
    public void StartPrep(float duration)
    {
        SetTime(duration, false);
    }

    private void SetTime(float duration, bool gameplay)
    {
        timeRemaining = Mathf.Max(0f, duration);
        isCountingDown = true;
        isGameplayTimer = gameplay;
        UpdateUI();
    }

    private void Update()
    {
        if (!isCountingDown)
            return;

        timeRemaining -= Time.deltaTime;
        if (timeRemaining < 0f)
        {
            timeRemaining = 0f;
            isCountingDown = false;
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);
            timeText.text = $"{minutes}:{seconds:00}";
        }
    }

    // Returns true only when the gameplay timer has finished
    public bool IsGameplayTimeUp()
    {
        return timeRemaining <= 0f && !isCountingDown && isGameplayTimer;
    }

    // Returns true when a preparation/countdown timer has finished
    public bool IsPrepFinished()
    {
        return timeRemaining <= 0f && !isCountingDown && !isGameplayTimer;
    }

    public float GetTimeRemaining() => timeRemaining;

    public void StopTimer()
    {
        timeRemaining = 0f;
        isCountingDown = false;
        UpdateUI();
    }

    // Explicitly set timer to zero and clear state (used when showing results)
    public void SetTimeZero()
    {
        timeRemaining = 0f;
        isCountingDown = false;
        isGameplayTimer = false;
        UpdateUI();
    }
}
