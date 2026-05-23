using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FloatingBar : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private Image bar;
    [SerializeField] private Image[] points;

    [Header("Smooth")]
    [SerializeField] private float lerpSpeed = 10f;

    [Header("Follow Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.5f, 0f);

    [SerializeField] private bool followRotation = false;

    private float targetFill = 1f;

    private Coroutine hideCoroutine;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        FollowTarget();

        UpdateBarSmooth();
    }

    private void FollowTarget()
    {
        if (target == null)
            return;

        Vector3 worldPosition =
            target.position + worldOffset;

        transform.position = worldPosition;

        if (!followRotation)
        {
            transform.rotation = Quaternion.identity;
        }
        else
        {
            transform.rotation = target.rotation;
        }
    }

    private void UpdateBarSmooth()
    {
        if (bar != null)
        {
            bar.fillAmount = Mathf.Lerp(
                bar.fillAmount,
                targetFill,
                Time.deltaTime * lerpSpeed
            );
        }
    }

    public void UpdateUI(int current, int max)
    {
        if (max <= 0)
            return;

        // Calculate and clamp target fill to avoid invalid values
        targetFill = Mathf.Clamp01((float)current / (float)max);

        // Immediately apply the fill so the bar matches current health
        if (bar != null)
        {
            bar.fillAmount = targetFill;
        }

        if (points != null && points.Length > 0)
        {
            for (int i = 0; i < points.Length; i++)
            {
                points[i].enabled =
                    ShouldShowPoint(
                        current,
                        max,
                        i,
                        points.Length
                    );
            }
        }
    }

    private bool ShouldShowPoint(
        int current,
        int max,
        int index,
        int totalPoints
    )
    {
        float step =
            (float)max / totalPoints;

        float threshold =
            step * (index + 1);

        return current >= threshold;
    }

    public void ShowTemporary(float duration)
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        hideCoroutine =
            StartCoroutine(
                HideAfterDelay(duration)
            );
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        gameObject.SetActive(false);

        hideCoroutine = null;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}