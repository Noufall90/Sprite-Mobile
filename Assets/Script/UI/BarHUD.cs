using UnityEngine;
using UnityEngine.UI;

public class BarHUD : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private Image bar;
    [SerializeField] private Image[] points;
    [SerializeField] private float lerpSpeed = 3f;

    protected virtual void Awake()
    {
        bar = GetComponent<Image>();
        points = GetComponentsInChildren<Image>();
    }

    public void UpdateUI(int current, int max)
    {
        if (max <= 0) return;

        float healthRatio = (float)current / max;

        if (bar != null)
        {
            bar.fillAmount = Mathf.Lerp(
                bar.fillAmount,
                healthRatio,
                lerpSpeed * Time.deltaTime
            );
        }

        if (points != null && points.Length > 0)
        {
            for (int i = 0; i < points.Length; i++)
            {
                points[i].enabled = ShouldShowPoint(
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
        int totalPoints)
    {
        float step = (float)max / totalPoints;
        float threshold = step * (index + 1);

        return current >= threshold;
    }
}