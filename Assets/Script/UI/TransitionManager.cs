using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TransitionManager : Singleton<TransitionManager>
{
    [Header("Fade Image")]
    [SerializeField] private Image fadeImage;

    [Header("Transition")]
    [SerializeField] private float fadeDuration = 1f;

    private Coroutine currentRoutine;

    protected override void Awake()
    {
        base.Awake();

        if (instance != this)
            return;

        // Keep existing parent (don't detach from Canvas) to ensure UI Image remains renderable

        if (fadeImage == null)
        {
            Debug.LogError("Fade Image belum di-assign!");
            return;
        }

        fadeImage.gameObject.SetActive(true);

        InstantBlack();
    }

    private IEnumerator Start()
    {
        yield return null;

        FadeOut();
    }

    public void FadeIn()
    {
        StartFade(1f);
    }

    public void FadeOut()
    {
        StartFade(0f);
    }

    public void LoadScene(string sceneName)
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine =
            StartCoroutine(
                LoadSceneRoutine(sceneName)
            );
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        yield return FadeRoutine(1f);

        AsyncOperation op =
            SceneManager.LoadSceneAsync(sceneName);

        while (!op.isDone)
        {
            yield return null;
        }

        yield return null;

        FadeOut();
    }

    private void StartFade(float targetAlpha)
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine =
            StartCoroutine(
                FadeRoutine(targetAlpha)
            );
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        fadeImage.gameObject.SetActive(true);

        float startAlpha = fadeImage.color.a;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float t = timer / fadeDuration;

            Color color = fadeImage.color;

            color.a =
                Mathf.Lerp(
                    startAlpha,
                    targetAlpha,
                    t
                );

            fadeImage.color = color;

            yield return null;
        }

        Color finalColor = fadeImage.color;
        finalColor.a = targetAlpha;
        fadeImage.color = finalColor;

        if (Mathf.Approximately(targetAlpha, 0f))
        {
            fadeImage.gameObject.SetActive(false);
        }

        currentRoutine = null;
    }

    public void InstantBlack()
    {
        fadeImage.gameObject.SetActive(true);

        Color color = fadeImage.color;
        color.a = 1f;
        fadeImage.color = color;
    }

    public void InstantClear()
    {
        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;

        fadeImage.gameObject.SetActive(false);
    }
}