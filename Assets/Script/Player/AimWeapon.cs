using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AimWeapon : MonoBehaviour
{
    private Camera mainCamera;

    [Header("Flashlight")]
    public Light2D flashlight;
    public float flashDuration = 0.05f;

    private Coroutine flashCoroutine;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (flashlight != null)
        {
            flashlight.enabled = false;
        }
    }

    private void Update()
    {
        HandleAiming();

        if (Input.GetMouseButtonDown(0))
        {
            TriggerFlashlight();
        }
    }

    private void HandleAiming()
    {
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        mousePosition.z = 0f;

        Vector3 aimDirection = (mousePosition - transform.position).normalized;

        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void TriggerFlashlight()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(FlashlightRoutine());
    }

    private IEnumerator FlashlightRoutine()
    {
        flashlight.enabled = true;

        yield return new WaitForSeconds(flashDuration);

        flashlight.enabled = false;
    }
}