using System.Collections;
using UnityEngine;

public class HealthPlayer : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] protected int maxHealth;
    [SerializeField] protected int currentHealth;

    [Header("UI")]
    [SerializeField] private BarHUD healthHUD;

    [Header("Death UI")]
    [SerializeField] private GameObject deathUI;
    [SerializeField] private float deathShowDuration = 0.6f;
    [Header("Death Visuals")]
    [SerializeField] private float desolveDuration = 1f;
    [SerializeField] private float desolveTarget = 1.1f;

    private bool isDead = false;

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
        if (healthHUD != null)
        {
            healthHUD.UpdateUI(currentHealth, maxHealth);
        }
    }

    private void Die()
    {
        isDead = true;

        Debug.Log("Player Dead");

        if (deathCoroutine != null)
        {
            StopCoroutine(deathCoroutine);
        }

        deathCoroutine = StartCoroutine(DissolveThenShowUI());
    }

    private Coroutine deathCoroutine;

    private IEnumerator ShowDeathUI()
    {
        if (deathUI == null)
        {
            yield break;
        }

        deathUI.SetActive(true);

        var t = deathUI.transform;
        t.localScale = Vector3.zero;

        float time = 0f;
        while (time < deathShowDuration)
        {
            time += Time.deltaTime;
            float k = Mathf.Lerp(0f, 1.1f, time / deathShowDuration);
            t.localScale = Vector3.one * k;
            yield return null;
        }

        t.localScale = Vector3.one * 1.1f;
    }

    private IEnumerator DissolveThenShowUI()
    {
        // Dissolve player visuals first
        yield return StartCoroutine(DissolveObject(gameObject, desolveDuration, desolveTarget));

        // Then show death UI with existing scale animation
        yield return StartCoroutine(ShowDeathUI());

        // Freeze game after death UI shown
        Time.timeScale = 0f;
    }

    private IEnumerator DissolveObject(GameObject targetObj, float duration, float targetVal)
    {
        if (targetObj == null)
            yield break;

        var renderers = targetObj.GetComponentsInChildren<Renderer>(true);

        float start = 0f;
        bool soundPlayed = false;
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
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float val = Mathf.Lerp(start, targetVal, t);

            foreach (var r in renderers)
            {
                if (r == null) continue;
                var mat = r.material;
                if (mat != null && mat.HasProperty("_DesolveAmount"))
                {
                    mat.SetFloat("_DesolveAmount", val);
                }
            }

            // Play death sound once when dissolve reaches 0.1
            if (!soundPlayed && val >= 0.1f)
            {
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySound2D("DeathPlayer");
                }
                soundPlayed = true;
            }

            yield return null;
        }

        foreach (var r in renderers)
        {
            if (r == null) continue;
            var mat = r.material;
            if (mat != null && mat.HasProperty("_DesolveAmount"))
            {
                mat.SetFloat("_DesolveAmount", targetVal);
            }
        }
    }
}