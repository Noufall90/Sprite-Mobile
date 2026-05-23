using System.Collections;
using UnityEngine;

public class EnemyShoot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyAI enemyAI;

    [SerializeField] private Transform firePoint;

    [SerializeField] private GameObject bulletPrefab;

    [Header("Shoot Settings")]
    [SerializeField] private float fireRate = 2f;

    [SerializeField] private float bulletSpeed = 10f;

    [SerializeField] private bool autoShoot = true;

    private bool canShoot = true;

    private Collider2D enemyCollider;

    private Transform target;

    private void Awake()
    {
        if (firePoint == null)
        {
            firePoint = transform;
        }

        if (enemyAI == null)
        {
            enemyAI = GetComponent<EnemyAI>();
        }

        enemyCollider = GetComponent<Collider2D>();

        FindPlayer();
    }

    private void Update()
    {
        if (!autoShoot)
            return;

        // Cari ulang jika player hilang
        if (target == null)
        {
            FindPlayer();
            return;
        }

        if (enemyAI == null)
            return;

        if (enemyAI.IsPlayerDetected())
        {
            Vector2 direction =
                ((Vector2)target.position -
                (Vector2)firePoint.position).normalized;

            Shoot(direction);
        }
    }

    // =========================
    // FIND PLAYER
    // =========================
    private void FindPlayer()
    {
        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            target = player.transform;
        }
    }

    // =========================
    // SHOOT
    // =========================
    public void Shoot(Vector2 direction)
    {
        if (!canShoot)
            return;

        StartCoroutine(ShootRoutine(direction));
    }

    private IEnumerator ShootRoutine(Vector2 direction)
    {
        canShoot = false;

        SpawnBullet(direction);

        yield return new WaitForSeconds(1f / fireRate);

        canShoot = true;
    }

    // =========================
    // BULLET
    // =========================
    private void SpawnBullet(Vector2 direction)
    {
        SoundManager.Instance.PlaySound2D("FireEnemy");
        if (bulletPrefab == null)
        {
            Debug.LogWarning("Bullet Prefab belum diisi.");
            return;
        }

        GameObject bullet =
            Instantiate(
                bulletPrefab,
                firePoint.position,
                Quaternion.identity
            );

        // ROTATION
        float angle =
            Mathf.Atan2(direction.y, direction.x) *
            Mathf.Rad2Deg;

        bullet.transform.rotation =
            Quaternion.Euler(0f, 0f, angle);

        // RIGIDBODY
        Rigidbody2D bulletRb =
            bullet.GetComponent<Rigidbody2D>();

        if (bulletRb != null)
        {
            bulletRb.gravityScale = 0f;

            bulletRb.velocity =
                direction.normalized *
                bulletSpeed;
        }

        // IGNORE SELF COLLISION
        Collider2D bulletCollider =
            bullet.GetComponent<Collider2D>();

        if (
            bulletCollider != null &&
            enemyCollider != null
        )
        {
            Physics2D.IgnoreCollision(
                bulletCollider,
                enemyCollider
            );
        }
    }

    // =========================
    // API
    // =========================
    public bool CanShoot()
    {
        return canShoot;
    }
}