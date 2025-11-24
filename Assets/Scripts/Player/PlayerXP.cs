using UnityEngine;

public class PlayerXP : MonoBehaviour
{
    [Header("XP Settings")]
    public int level = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 5;
    public float xpGrowthFactor = 1.5f;

    [Header("References")]
    public UpgradeSelector upgradeUI;   // NOTE: UI script, not AbilityUpgradeSelector

    void OnEnable()
    {
        EnemyHealth.OnAnyEnemyDied += OnEnemyDied;
    }

    void OnDisable()
    {
        EnemyHealth.OnAnyEnemyDied -= OnEnemyDied;
    }

    void Awake()
    {
        // Auto-find UI upgrade selector if not assigned
        if (upgradeUI == null)
        {
            upgradeUI = FindObjectOfType<UpgradeSelector>();
            if (upgradeUI == null)
                Debug.LogWarning("PlayerXP: No UpgradeSelector (UI) found in scene.");
        }
    }

    private void OnEnemyDied(int xpGained)
    {
        GainXP(xpGained);
    }

    public void GainXP(int amount)
    {
        currentXP += amount;
        Debug.Log($"XP gained: {amount}. Total XP: {currentXP}/{xpToNextLevel}");

        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        level++;
        Debug.Log($"LEVEL UP! New level: {level}");

        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * xpGrowthFactor);

        // 🚀 Now we queue a UI choice instead of auto-applying
        if (upgradeUI != null)
        {
            upgradeUI.QueueUpgrade();
        }
        else
        {
            Debug.LogWarning("PlayerXP: upgradeUI is null, cannot show upgrade choices.");
        }
    }
}
