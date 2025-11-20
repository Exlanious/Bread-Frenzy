using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 5;
    private int currentHealth;

    [Header("UI")]
    public ProgressBar healthBar;
    public bool hideWhenFull = true;

    [Header("Death Settings")]
    public bool destroyOnDeath = true;
    public float destroyDelay = 0.5f;

    public event Action<EnemyHealth> OnEnemyDied;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"{name} took {amount} damage! Health = {currentHealth}/{maxHealth}");

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBar == null) return;

        float progress = (float)currentHealth / maxHealth;
        healthBar.SetProgress(progress);

        if (hideWhenFull)
            healthBar.gameObject.SetActive(progress < 1f);
    }

    private void Die()
    {
        // Disable health bar
        if (healthBar != null)
            healthBar.gameObject.SetActive(false);

        if (destroyOnDeath)
            Destroy(gameObject, destroyDelay);
    }

    void OnDestroy()
    {
        Debug.Log($"{name} has died!");
        OnEnemyDied?.Invoke(this);
    }
}
