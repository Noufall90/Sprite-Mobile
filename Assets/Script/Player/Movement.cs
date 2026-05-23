using System.Collections;
using UnityEngine;

public class movement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public Rigidbody2D rb;

    private Vector2 moveDirection;

    [Header("Dash")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.5f;
    private bool isDashing;
    private bool canDash = true;

    [Header("Effects")]
    public ParticleSystem dust;

    void Update()
    {
        ProcessInput();

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
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector2(moveX, moveY).normalized;
    }

    void Move()
    {
        rb.velocity = moveDirection * moveSpeed;
    }

    IEnumerator Dash()
    {
        SoundManager.Instance.PlaySound2D("DashPlayer");
        canDash = false;
        isDashing = true;

        dust.Play();

        rb.velocity = moveDirection * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }
}