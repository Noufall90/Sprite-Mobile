using System.Collections;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Header("Transition")]
    [SerializeField] private bool playFadeOutOnStart = true;

    [SerializeField] private float fadeDelay = 0.1f;

    public int score;

    protected override void Awake()
    {
        base.Awake();

        // Pastikan instance TransitionManager ada
        if (
            playFadeOutOnStart &&
            TransitionManager.Instance != null
        )
        {
            StartCoroutine(StartTransition());
        }
    }

    private IEnumerator StartTransition()
    {
        // Tunggu 1 frame agar UI selesai initialize
        yield return null;

        // Optional delay kecil
        yield return new WaitForSeconds(fadeDelay);

        // Fade dari hitam -> transparan
        TransitionManager.Instance.FadeOut();
    }
}