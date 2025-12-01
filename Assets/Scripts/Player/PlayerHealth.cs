using UnityEngine;
using System;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("UI")]
    [Tooltip("Optional UI bar to display player health.")]
    public ProgressBar healthBar;
    public bool hideWhenFull = false;

    [Header("Options")]
    [Tooltip("If true, log damage and death events in the console.")]
    public bool debugLogs = false;

    [Header("Damage Timing")]
    public float invincibilityDuration = 0.5f;   
    private float lastDamageTime = -999f;

    [Header("Feedback")]
    public Renderer playerRenderer;
    public Color hurtFlashColor = Color.red;
    public float flashSpeed = 20f;
    public float hitstopDuration = 0.05f;

    private bool flashing = false;
    private bool inHitstop = false;

    public event Action OnPlayerDied;
    public event Action<int, int> OnHealthChanged;

    private void Start()
    {
        ResetHealth();
    }

    public void TakeDamage(int amount)
    {
        if (Time.time - lastDamageTime < invincibilityDuration)
            return;

        lastDamageTime = Time.time;

        if (!flashing)
            StartCoroutine(FlashRoutine());

        if (!inHitstop)
            StartCoroutine(HitstopRoutine());

        if (RunStats.Instance != null && amount > 0)
        {
            RunStats.Instance.RegisterDamageTaken(amount);
        }

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (debugLogs)
            Debug.Log($"[PlayerHealth] Took {amount} damage. Health = {currentHealth}/{maxHealth}");

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }

        UpdateHealthBar();
    }

    private IEnumerator FlashRoutine()
    {
        if (playerRenderer == null || playerRenderer.material == null)
            yield break;

        flashing = true;

        Color originalColor = playerRenderer.material.color;

        while (Time.time - lastDamageTime < invincibilityDuration)
        {
            float t = Mathf.Sin(Time.time * flashSpeed) * 0.5f + 0.5f;
            playerRenderer.material.color = Color.Lerp(originalColor, hurtFlashColor, t);
            yield return null;
        }

        playerRenderer.material.color = originalColor;
        flashing = false;
    }

    private IEnumerator HitstopRoutine()
    {
        inHitstop = true;

        PauseManager.SetPaused(PauseSource.Hitstop, true);

        yield return new WaitForSecondsRealtime(hitstopDuration);

        PauseManager.SetPaused(PauseSource.Hitstop, false);
        inHitstop = false;
    }

    private void Die()
    {
        if (debugLogs)
            Debug.Log("[PlayerHealth] Player died!");

        OnPlayerDied?.Invoke();
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        UpdateHealthBar();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (healthBar == null) return;

        float progress = (float)currentHealth / maxHealth;
        healthBar.SetProgress(progress);

        if (hideWhenFull)
            healthBar.gameObject.SetActive(progress < 1f);
    }
}
