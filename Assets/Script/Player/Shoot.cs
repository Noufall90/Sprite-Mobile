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
    [Header("External Fire")]
    public bool useExternalFire = true; // mobile-first: shooting triggered externally (e.g., joystick)
    private bool externalFiring = false;

    private void Awake()
    {
        readyForNextShot = 0f;
        fireRate = Mathf.Max(fireRate, 0.0001f);
    }

    void Update()
    {
        if (Time.timeScale <= 0f)
            return;

        if (WaveUI.Instance != null && WaveUI.Instance.IsResultsVisible())
            return;

        if (useExternalFire)
        {
            if (externalFiring && Time.time >= readyForNextShot)
            {
                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlaySound2D("FirePlayer");
                shoot();
                readyForNextShot = Time.time + (1f / Mathf.Max(fireRate, 0.0001f));
            }
            return;
        }
    }

    public void SetExternalFiring(bool firing)
    {
        externalFiring = firing;
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
