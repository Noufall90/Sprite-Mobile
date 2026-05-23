using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class movement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public Rigidbody2D rb;

    private Vector2 moveDirection;

    [Header("Joystick")]
    public GameObject movementJoystick; // assign the on-screen joystick GameObject

    [Header("Dash")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.5f;
    private bool isDashing;
    private bool canDash = true;
    public GameObject dashButton; // assign a UI Button GameObject for dash

    [Header("Effects")]
    public ParticleSystem dust;

    private Button dashUIButton;

    void Start()
    {
        if (dashButton != null)
        {
            dashUIButton = dashButton.GetComponent<Button>();
            if (dashUIButton != null)
            {
                dashUIButton.onClick.AddListener(() =>
                {
                    if (canDash && moveDirection != Vector2.zero)
                        StartCoroutine(Dash());
                });
            }
        }
    }

    void Update()
    {
        ProcessInput();

        // keyboard fallback for dash
        if (Input.GetKeyDown(KeyCode.Space) && canDash && moveDirection != Vector2.zero)
        {
            StartCoroutine(Dash());
        }
    }

    void FixedUpdate()
    {
        if (!isDashing)
        {
            Move();
        }
    }

    void ProcessInput()
    {
        Vector2 joystick = Vector2.zero;
        if (movementJoystick != null)
        {
            joystick = ReadJoystick(movementJoystick);
        }

        if (joystick != Vector2.zero)
        {
            moveDirection = joystick.normalized;
        }
        else
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");
            moveDirection = new Vector2(moveX, moveY).normalized;
        }
    }

    void Move()
    {
        if (rb != null)
            rb.velocity = moveDirection * moveSpeed;
    }

    IEnumerator Dash()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound2D("DashPlayer");
        canDash = false;
        isDashing = true;

        if (dust != null)
            dust.Play();

        if (rb != null)
            rb.velocity = moveDirection * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    // Reflection-based joystick reader to avoid compile dependency on joystick package
    private Vector2 ReadJoystick(GameObject joystickObj)
    {
        if (joystickObj == null) return Vector2.zero;

        var comps = joystickObj.GetComponents<MonoBehaviour>();
        foreach (var comp in comps)
        {
            var t = comp.GetType();

            // common properties: Horizontal / Vertical
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

            // some joysticks expose a Vector2 'Direction' or 'inputDirection'
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

            // check fields too
            var fieldH = t.GetField("Horizontal") ?? t.GetField("horizontal");
            var fieldV = t.GetField("Vertical") ?? t.GetField("vertical");
            if (fieldH != null && fieldV != null)
            {
                try
                {
                    float h = Convert.ToSingle(fieldH.GetValue(comp));
                    float v = Convert.ToSingle(fieldV.GetValue(comp));
                    return new Vector2(h, v);
                }
                catch { }
            }
        }

        return Vector2.zero;
    }
}