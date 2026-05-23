using System;
using System.Reflection;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AimWeapon : MonoBehaviour
{
    private Camera mainCamera;

    [Header("Flashlight")]
    public Light2D flashlight;
    public float flashDuration = 0.05f;

    [Header("Joystick Aim")]
    public GameObject aimJoystick; // assign on-screen joystick GameObject for aiming
    public float aimDeadzone = 0.2f;

    private Coroutine flashCoroutine;
    private Shoot shootComponent;
    private Vector2 lastJoystickDirection = Vector2.zero;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (flashlight != null)
        {
            flashlight.enabled = false;
        }

        // find Shoot component (on same GameObject or children)
        shootComponent = GetComponent<Shoot>() ?? GetComponentInChildren<Shoot>() ?? GetComponentInParent<Shoot>();
        if (shootComponent != null)
        {
            // enable external firing mode
            shootComponent.useExternalFire = true;
        }
    }

    private void Update()
    {
        HandleAiming();
    }

    private void HandleAiming()
    {
        Vector2 aimDir = Vector2.zero;
        bool usingJoystick = false;

        if (aimJoystick != null)
        {
            aimDir = ReadJoystick(aimJoystick);
            if (aimDir.magnitude > aimDeadzone)
            {
                usingJoystick = true;
                lastJoystickDirection = aimDir.normalized;
            }
        }

        if (usingJoystick)
        {
            Vector3 aimDirection = new Vector3(lastJoystickDirection.x, lastJoystickDirection.y, 0f);
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            // trigger shooting while joystick is held
            if (shootComponent != null)
            {
                shootComponent.SetExternalFiring(true);
            }
        }
        else
        {
            // No joystick input: stop external firing
            if (shootComponent != null)
            {
                shootComponent.SetExternalFiring(false);
            }
        }
    }

    private void OnDisable()
    {
        if (shootComponent != null)
            shootComponent.SetExternalFiring(false);
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

    // Reflection-based joystick reader (same approach as Movement)
    private Vector2 ReadJoystick(GameObject joystickObj)
    {
        if (joystickObj == null) return Vector2.zero;

        var comps = joystickObj.GetComponents<MonoBehaviour>();
        foreach (var comp in comps)
        {
            var t = comp.GetType();

            var propH = t.GetProperty("Horizontal") ?? t.GetProperty("horizontal");
            var propV = t.GetProperty("Vertical") ?? t.GetProperty("vertical");
            if (propH != null && propV != null)
            {
                try
                {
                    float h = Convert.ToSingle(propH.GetValue(comp, null));
                    float v = Convert.ToSingle(propV.GetValue(comp, null));
                    return new Vector2(h, v);
                }
                catch { }
            }

            var propDir = t.GetProperty("Direction") ?? t.GetProperty("direction") ?? t.GetProperty("inputDirection");
            if (propDir != null)
            {
                try
                {
                    var dirVal = propDir.GetValue(comp, null);
                    if (dirVal is Vector2 v2) return v2;
                    if (dirVal is Vector3 v3) return new Vector2(v3.x, v3.y);
                }
                catch { }
            }
        }

        return Vector2.zero;
    }
}