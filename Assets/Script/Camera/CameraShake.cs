using System.Collections;
using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    public bool start = false;
    public AnimationCurve curve;
    public float duration = 0.5f;

    [Header("Cinemachine (optional)")]
    public CinemachineVirtualCamera cinemachineCamera;
    public float cinemachineAmplitude = 2f;
    public float cinemachineFrequency = 1f;

    void Update()
    {
        if (start)
        {
            start = false;
            TriggerShake();
        }
    }

    public void TriggerShake()
    {
        StopAllCoroutines();
        StartCoroutine(Shaking());
    }

    IEnumerator Shaking()
    {
        // If a Cinemachine virtual camera is provided, drive its Perlin noise component.
        CinemachineBasicMultiChannelPerlin perlin = null;
        if (cinemachineCamera != null)
        {
            perlin = cinemachineCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float strength = curve.Evaluate(elapsedTime / duration);

            if (perlin != null)
            {
                perlin.m_AmplitudeGain = strength * cinemachineAmplitude;
                perlin.m_FrequencyGain = strength * cinemachineFrequency;
            }    
            yield return null;
        }

        // restore
        if (perlin != null)
        {
            perlin.m_AmplitudeGain = 0f;
            perlin.m_FrequencyGain = 0f;
        }
    }
}
