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
    private bool isDead = false;
    [Header("Death Visuals")]
    [SerializeField] private float desolveDuration = 1f;
    [SerializeField] private float desolveTarget = 1.1f;

    // Event untuk kematian
    public event Action OnDeath;
    // Global event for scoring: sends points awarded
    public static Action<int> OnEnemyKilled;

    private void Start()
    {
        currentHealth = maxHealth;

        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

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
        if (isDead) return;

        currentHealth += amount;

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (floatingBar != null)
        {
            floatingBar.UpdateUI(currentHealth, maxHealth);
        }
    }

    private void Die()
    {
        isDead = true;

        OnDeath?.Invoke();

        OnEnemyKilled?.Invoke(10);

        CameraShake camShake = GetComponentInParent<CameraShake>();
        if (camShake == null)
        {
            camShake = FindObjectOfType<CameraShake>();
        }

        if (camShake != null)
        {
            camShake.TriggerShake();
        }

        GameObject toDestroy = destroyRootOnDeath ? transform.root.gameObject : gameObject;
        StartCoroutine(DissolveAndDestroy(toDestroy, desolveDuration));
    }

    private IEnumerator DissolveAndDestroy(GameObject toDestroy, float duration)
    {
        if (toDestroy == null)
            yield break;
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D("DeathEnemy");
        }

        var renderers = toDestroy.GetComponentsInChildren<Renderer>();

        float start = 0f;
        foreach (var r in renderers)
        {
            if (r == null) continue;
            var mat = r.material;
            if (mat != null && mat.HasProperty("_DesolveAmount"))
            {
                start = mat.GetFloat("_DesolveAmount");
                break;
            }
        }

        float elapsed = 0f;
        bool stoppedMovement = false;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float val = Mathf.Lerp(start, desolveTarget, t);

            foreach (var r in renderers)
            {
                if (r == null) continue;
                var mat = r.material;
                if (mat != null && mat.HasProperty("_DesolveAmount"))
                {
                    mat.SetFloat("_DesolveAmount", val);
                }
            }

            if (!stoppedMovement && val >= 0.9f)
            {
                var rbs = toDestroy.GetComponentsInChildren<Rigidbody2D>();
                foreach (var rb in rbs)
                {
                    if (rb == null) continue;
                    rb.velocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                    rb.simulated = false;
                }

                // Disable EnemyAI scripts so they won't resume movement
                var ais = toDestroy.GetComponentsInChildren<EnemyAI>();
                foreach (var ai in ais)
                {
                    if (ai == null) continue;
                    ai.enabled = false;
                }

                // Disable EnemyShoot scripts so they stop firing
                var shoots = toDestroy.GetComponentsInChildren<EnemyShoot>();
                foreach (var s in shoots)
                {
                    if (s == null) continue;
                    s.enabled = false;
                }

                stoppedMovement = true;
            }

            yield return null;
        }

        foreach (var r in renderers)
        {
            if (r == null) continue;
            var mat = r.material;
            if (mat != null && mat.HasProperty("_DesolveAmount"))
            {
                mat.SetFloat("_DesolveAmount", desolveTarget);
            }
        }
        Destroy(toDestroy);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            int damageTaken = 0;

            Bullet bulletComp = other.GetComponent<Bullet>();
            if (bulletComp != null)
            {
                damageTaken = bulletComp.damage;
            }
            else
            {
                Shoot bulletShoot = other.GetComponent<Shoot>();
                if (bulletShoot != null)
                {
                    damageTaken = bulletShoot.damage;
                }
                else
                {
                    var comps = other.GetComponents<MonoBehaviour>();
                    foreach (var c in comps)
                    {
                        if (c == null) continue;
                        var dmgField = c.GetType().GetField("damage");
                        if (dmgField != null)
                        {
                            object val = dmgField.GetValue(c);
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
}