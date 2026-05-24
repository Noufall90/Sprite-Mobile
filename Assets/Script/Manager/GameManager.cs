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
        yield return null;
        yield return new WaitForSeconds(fadeDelay);
        TransitionManager.Instance.FadeOut();
    }
}