using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float bulletSpeed;
    public Transform firePoint;
    public float fireRate;
    public int damage;
    private float readyForNextShot = 0f;
    public Animator gunAnimator;

    private void Awake()
    {
        readyForNextShot = 0f;
        fireRate = Mathf.Max(fireRate, 0.0001f);
    }

    void Update()
    {
        // Do not allow shooting while the game is paused or wave results are showing
        if (Time.timeScale <= 0f)
            return;

        if (WaveUI.Instance != null && WaveUI.Instance.IsResultsVisible())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            SoundManager.Instance.PlaySound2D("FirePlayer");
            shoot();
            readyForNextShot = Time.time + (1f / Mathf.Max(fireRate, 0.0001f));
            return;
        }

        if (Input.GetMouseButton(0) && Time.time >= readyForNextShot)
        {
            shoot();
            readyForNextShot = Time.time + (1f / Mathf.Max(fireRate, 0.0001f));
        }
    }

    void shoot()
    {
        GameObject BulletIns = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = BulletIns.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = firePoint.up * bulletSpeed;
        }
        gunAnimator.SetTrigger("Shoot");
        Destroy(BulletIns, 1.5f);
    }
}
