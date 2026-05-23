using UnityEngine;
using System;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected int currentHealth;

    [Header("Death Settings")]
    [SerializeField] private bool destroyRootOnDeath = true;

    [Header("UI")]
    [SerializeField] private FloatingBar floatingBar;

    [Header("Death Visuals")]
    [SerializeField] private float desolveDuration = 1f;
    [SerializeField] private float desolveTarget = 1.1f;

    private bool isDead = false;
    private EnemySpawn ownerSpawn;
    public event Action OnDeath;
    public static Action<int> OnEnemyKilled;

    private void Start()
    {
        currentHealth = maxHealth;

        UpdateHealthUI();
    }

    public void SetOwnerSpawn(EnemySpawn spawn)
    {
        ownerSpawn = spawn;
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        currentHealth =
            Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();

        if (floatingBar != null)
        {
            floatingBar.ShowTemporary(2f);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (isDead)
            return;

        currentHealth += amount;

        currentHealth =
            Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (floatingBar != null)
        {
            floatingBar.UpdateUI(
                currentHealth,
                maxHealth
            );
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        OnDeath?.Invoke();

        OnEnemyKilled?.Invoke(10);

        CameraShake camShake =
            GetComponentInParent<CameraShake>();

        if (camShake == null)
        {
            camShake =
                FindObjectOfType<CameraShake>();
        }

        if (camShake != null)
        {
            camShake.TriggerShake();
        }

        GameObject toDestroy =
            destroyRootOnDeath
            ? transform.root.gameObject
            : gameObject;

        StartCoroutine(
            DissolveAndDestroy(
                toDestroy,
                desolveDuration
            )
        );
    }

    private IEnumerator DissolveAndDestroy(
        GameObject toDestroy,
        float duration
    )
    {
        if (toDestroy == null)
            yield break;

        // Sound mati
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D("DeathEnemy");
        }

        Collider2D[] colliders =
            toDestroy.GetComponentsInChildren<Collider2D>();

        foreach (var col in colliders)
        {
            if (col != null)
            {
                col.enabled = false;
            }
        }

        // Stop movement
        Rigidbody2D[] rbs =
            toDestroy.GetComponentsInChildren<Rigidbody2D>();

        foreach (var rb in rbs)
        {
            if (rb == null)
                continue;

            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        // Disable AI
        EnemyAI[] ais =
            toDestroy.GetComponentsInChildren<EnemyAI>();

        foreach (var ai in ais)
        {
            if (ai != null)
            {
                ai.enabled = false;
            }
        }

        EnemyShoot[] shoots =
            toDestroy.GetComponentsInChildren<EnemyShoot>();

        foreach (var s in shoots)
        {
            if (s != null)
            {
                s.enabled = false;
            }
        }

        Renderer[] renderers =
            toDestroy.GetComponentsInChildren<Renderer>();

        float start = 0f;

        foreach (var r in renderers)
        {
            if (r == null)
                continue;

            Material mat = r.material;

            if (mat != null &&
                mat.HasProperty("_DesolveAmount"))
            {
                start =
                    mat.GetFloat("_DesolveAmount");

                break;
            }
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(elapsed / duration);

            float val =
                Mathf.Lerp(
                    start,
                    desolveTarget,
                    t
                );

            foreach (var r in renderers)
            {
                if (r == null)
                    continue;

                Material mat = r.material;

                if (mat != null &&
                    mat.HasProperty("_DesolveAmount"))
                {
                    mat.SetFloat(
                        "_DesolveAmount",
                        val
                    );
                }
            }

            yield return null;
        }

        // Final dissolve value
        foreach (var r in renderers)
        {
            if (r == null)
                continue;

            Material mat = r.material;

            if (mat != null &&
                mat.HasProperty("_DesolveAmount"))
            {
                mat.SetFloat(
                    "_DesolveAmount",
                    desolveTarget
                );
            }
        }

        // Beritahu spawner bahwa enemy sudah mati
        if (ownerSpawn != null)
        {
            ownerSpawn.ClearCurrentEnemy(toDestroy);
        }

        Destroy(toDestroy);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Bullet"))
            return;

        int damageTaken = 0;

        Bullet bulletComp =
            other.GetComponent<Bullet>();

        if (bulletComp != null)
        {
            damageTaken = bulletComp.damage;
        }
        else
        {
            Shoot bulletShoot =
                other.GetComponent<Shoot>();

            if (bulletShoot != null)
            {
                damageTaken = bulletShoot.damage;
            }
            else
            {
                MonoBehaviour[] comps =
                    other.GetComponents<MonoBehaviour>();

                foreach (var c in comps)
                {
                    if (c == null)
                        continue;

                    var dmgField =
                        c.GetType().GetField("damage");

                    if (dmgField != null)
                    {
                        object val =
                            dmgField.GetValue(c);

                        if (val is int)
                        {
                            damageTaken = (int)val;
                            break;
                        }
                    }
                }
            }
        }

        if (damageTaken > 0)
        {
            TakeDamage(damageTaken);
        }

        Destroy(other.gameObject);
    }
}