using UnityEngine;
using System;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected int currentHealth;
    [Header("Death Settings")]
    [Tooltip("If true, destroy the root GameObject (top-level) on death. Otherwise destroy the GameObject that has this component.")]
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

        Debug.Log("Enemy Dead");

        // Trigger OnDeath event
        OnDeath?.Invoke();

        // award points to global listeners
        OnEnemyKilled?.Invoke(10);

        // Try to find a CameraShake component on parent hierarchy first,
        // otherwise fall back to the first CameraShake in the scene.
        CameraShake camShake = GetComponentInParent<CameraShake>();
        if (camShake == null)
        {
            camShake = FindObjectOfType<CameraShake>();
        }

        if (camShake != null)
        {
            camShake.TriggerShake();
        }

        // Destroy the appropriate GameObject after a dissolve effect
        GameObject toDestroy = destroyRootOnDeath ? transform.root.gameObject : gameObject;
        StartCoroutine(DissolveAndDestroy(toDestroy, desolveDuration));
    }

    private IEnumerator DissolveAndDestroy(GameObject toDestroy, float duration)
    {
        if (toDestroy == null)
            yield break;

        // Collect renderers under the object
        var renderers = toDestroy.GetComponentsInChildren<Renderer>();

        // Read initial value from first renderer that has the property
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
        bool soundPlayed = false;
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

            // Play enemy death sound once when dissolve reaches 0.1
            if (!soundPlayed && val >= 0.1f)
            {
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySound2D("DeathEnemy");
                }
                soundPlayed = true;
            }

            yield return null;
        }

        // ensure final value
        foreach (var r in renderers)
        {
            if (r == null) continue;
            var mat = r.material;
            if (mat != null && mat.HasProperty("_DesolveAmount"))
            {
                mat.SetFloat("_DesolveAmount", desolveTarget);
            }
        }

        // In case the loop finished without hitting 0.1 (very short durations), play sound if needed
        if (!soundPlayed && desolveTarget >= 0.1f)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySound2D("DeathEnemy");
            }
        }

        Destroy(toDestroy);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            // Try to get damage from a Shoot or Bullet component on the projectile
            int damageTaken = 0;

            // Prefer a dedicated Bullet component on the projectile
            Bullet bulletComp = other.GetComponent<Bullet>();
            if (bulletComp != null)
            {
                damageTaken = bulletComp.damage;
            }
            else
            {
                // Fallback: check for Shoot component (if the player-script was placed on projectile)
                Shoot bulletShoot = other.GetComponent<Shoot>();
                if (bulletShoot != null)
                {
                    damageTaken = bulletShoot.damage;
                }
                else
                {
                    // support other projectile components by scanning for a public 'damage' field
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