using UnityEngine;
using System;

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

    public event Action OnPlayerDied;
    public event Action<int, int> OnHealthChanged;

    private void Start()
    {
        ResetHealth();
    }

    // ---------------------------------------------------------------
    // DAMAGE & DEATH
    // ---------------------------------------------------------------
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (debugLogs)
            Debug.Log($"[PlayerHealth] Took {amount} damage. Health = {currentHealth}/{maxHealth}");

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }


    private void Die()
    {
        if (debugLogs)
            Debug.Log("[PlayerHealth] Player died!");

        OnPlayerDied?.Invoke();
    }

    // ---------------------------------------------------------------
    // HEALTH CONTROL
    // ---------------------------------------------------------------
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

    // ---------------------------------------------------------------
    // UI UPDATE
    // ---------------------------------------------------------------
    private void UpdateHealthBar()
    {
        if (healthBar == null) return;

        float progress = (float)currentHealth / maxHealth;
        healthBar.SetProgress(progress);

        if (hideWhenFull)
            healthBar.gameObject.SetActive(progress < 1f);
    }
}
