using System.Collections;
using UnityEngine;

public class PlayerXP : MonoBehaviour
{
    [Header("XP Settings")]
    public int level = 1;

    // XP in the *current* level
    public int currentXP = 0;
    public int xpToNextLevel = 5;
    public float xpGrowthFactor = 1.5f;

    [Header("References")]
    public UpgradeSelector upgradeUI;   // upgrade choice UI
    public XPBarUI xpBarUI;             // circular XP UI

    // Internal
    private int xpQueue = 0;            // XP waiting to be animated
    private bool isAnimatingXP = false;
    private int totalXP = 0;            // optional: total across all levels

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
        if (upgradeUI == null)
            upgradeUI = FindObjectOfType<UpgradeSelector>();

        if (xpBarUI == null)
            xpBarUI = FindObjectOfType<XPBarUI>();
    }

    void Start()
    {
        // Initialize UI once
        UpdateXPUI();
    }

    private void OnEnemyDied(int xpGained)
    {
        GainXP(xpGained);
    }

    public void GainXP(int amount)
    {
        // 1. Gain XP internally
        totalXP += amount;
        xpQueue += amount;

        // 2. Start animation if not already running
        if (!isAnimatingXP)
            StartCoroutine(ProcessXPQueue());
    }

    private IEnumerator ProcessXPQueue()
    {
        isAnimatingXP = true;

        while (xpQueue > 0)
        {
            // How much XP we can add before hitting the next level
            int xpNeededThisLevel = xpToNextLevel - currentXP;
            int chunk = Mathf.Min(xpQueue, xpNeededThisLevel);

            xpQueue -= chunk;

            // 2. Animate circle from currentXP → currentXP + chunk
            float startXP = currentXP;
            float endXP   = currentXP + chunk;

            // Tune this for speed of animation
            float duration = 0.35f;
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                float lerpXP = Mathf.Lerp(startXP, endXP, t / duration);

                // Update bar using intermediate value
                UpdateXPUI(lerpXP);

                yield return null;
            }

            // Snap to final chunk value
            currentXP += chunk;
            UpdateXPUI();

            // 3. After animation, check for level-up
            if (currentXP >= xpToNextLevel)
            {
                currentXP -= xpToNextLevel;
                LevelUp();          // this queues the upgrade UI
                // xpToNextLevel is updated inside LevelUp()
            }
        }

        isAnimatingXP = false;
    }

    private void LevelUp()
    {
        level++;
        Debug.Log($"LEVEL UP! New level: {level}");

        // Increase requirement for next level
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * xpGrowthFactor);

        // Trigger upgrade choice UI
        if (upgradeUI != null)
        {
            upgradeUI.QueueUpgrade();
        }
        else
        {
            Debug.LogWarning("PlayerXP: upgradeUI is null, cannot show upgrade choices.");
        }

        // Refresh UI with new level & thresholds
        UpdateXPUI();
    }

    private void UpdateXPUI()
    {
        if (xpBarUI != null)
            xpBarUI.SetXP(currentXP, xpToNextLevel, level);
    }

    // Overload used during animation
    private void UpdateXPUI(float currentXPOverride)
    {
        if (xpBarUI != null)
            xpBarUI.SetXP(currentXPOverride, xpToNextLevel, level);
    }
}
