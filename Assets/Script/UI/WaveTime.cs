using UnityEngine;
using TMPro;

public class WaveTime : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text timeText;

    [Header("Wave Settings")]
    public float timeWave;
    public float timeAddWave;
    [Tooltip("Batas wave untuk penambahan waktu. Wave di atas ini menggunakan durasi sama dengan wave terakhir yang dibatasi.")]
    public int maxAddWave = 3;

    private float timeRemaining = 0f;
    private bool isCountingDown = false;
    private bool isGameplayTimer = false;
    private bool prepStarted = false;

    private void Start()
    {
        timeRemaining = 0f;
        isCountingDown = false;
        prepStarted = false;
        UpdateUI();
        Debug.Log($"[WaveTime] Start — timeWave: {timeWave}, timeAddWave: {timeAddWave}, maxAddWave: {maxAddWave}");
    }

    /// <summary>
    /// Memulai gameplay timer.
    /// waveNumber dimulai dari 0 (wave pertama = 0, kedua = 1, dst.)
    ///
    /// Durasi tiap wave:
    ///   wave 0 → timeWave + 0 * timeAddWave
    ///   wave 1 → timeWave + 1 * timeAddWave
    ///   wave 2 → timeWave + 2 * timeAddWave
    ///   wave 3+ → timeWave + maxAddWave * timeAddWave  (dibatasi)
    /// </summary>
    public void StartGameplayTimer(int waveNumber = 0)
    {
        // Clamp agar penambahan waktu tidak melebihi batas maxAddWave wave
        int clampedWave = Mathf.Clamp(waveNumber, 0, maxAddWave);
        float duration  = timeWave + clampedWave * timeAddWave;

        Debug.Log($"[WaveTime] StartGameplayTimer wave {waveNumber} (clamped: {clampedWave}) — durasi: {duration}s");
        SetTime(duration, true);
    }

    // Memulai timer persiapan sebelum wave dimulai
    public void StartPrep(float duration)
    {
        Debug.Log($"[WaveTime] StartPrep — durasi: {duration}s");
        prepStarted = true;
        SetTime(duration, false);
    }

    private void SetTime(float duration, bool gameplay)
    {
        timeRemaining  = Mathf.Max(0f, duration);
        isCountingDown = (timeRemaining > 0f);
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
            timeRemaining  = 0f;
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

    // Mengembalikan true hanya saat gameplay timer habis
    public bool IsGameplayTimeUp()
    {
        return isGameplayTimer && !isCountingDown && timeRemaining <= 0f;
    }

    // Mengembalikan true saat prep timer selesai (butuh prepStarted = true)
    public bool IsPrepFinished()
    {
        return prepStarted && !isGameplayTimer && !isCountingDown && timeRemaining <= 0f;
    }

    public float GetTimeRemaining() => timeRemaining;

    public void StopTimer()
    {
        timeRemaining  = 0f;
        isCountingDown = false;
        UpdateUI();
    }

    public void SetTimeZero()
    {
        timeRemaining   = 0f;
        isCountingDown  = false;
        isGameplayTimer = false;
        prepStarted     = false;
        UpdateUI();
    }
}
