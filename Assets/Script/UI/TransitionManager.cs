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

        if (fadeImage == null)
        {
            Debug.LogError("Fade Image belum di-assign!");
            return;
        }

        fadeImage.gameObject.SetActive(true);
        SetAlpha255(255f);
    }

    private IEnumerator Start()
    {
        yield return null;

        FadeOut();
    }

    public void FadeIn()
    {
        if (!fadeImage.gameObject.activeInHierarchy)
            fadeImage.gameObject.SetActive(true);

        StartFade(255f);
    }

    public void FadeOut()
    {
        if (!fadeImage.gameObject.activeInHierarchy)
            fadeImage.gameObject.SetActive(true);

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
        yield return FadeRoutine(255f);
        SceneManager.LoadScene(sceneName);
    }

    private void StartFade(float targetAlpha255)
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        if (!fadeImage.gameObject.activeInHierarchy)
            fadeImage.gameObject.SetActive(true);

        currentRoutine = StartCoroutine(FadeRoutine(targetAlpha255));
    }

    private IEnumerator FadeRoutine(float targetAlpha255)
    {
        float startAlpha255 =
            fadeImage.color.a * 255f;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / fadeDuration
                );

            float alpha255 =
                Mathf.Lerp(
                    startAlpha255,
                    targetAlpha255,
                    t
                );

            SetAlpha255(alpha255);

            yield return null;
        }

        SetAlpha255(targetAlpha255);

        if (Mathf.Approximately(targetAlpha255, 0f))
        {
            fadeImage.gameObject.SetActive(false);
        }

        currentRoutine = null;
    }

    private void SetAlpha255(float alpha255)
    {
        Color color = fadeImage.color;

        color.a =
            Mathf.Clamp01(alpha255 / 255f);

        fadeImage.color = color;
    }
    
    public void InstantBlack()
    {
        fadeImage.gameObject.SetActive(true);
        SetAlpha255(255f);
    }

    public void InstantClear()
    {
        SetAlpha255(0f);
        fadeImage.gameObject.SetActive(false);
    }
}