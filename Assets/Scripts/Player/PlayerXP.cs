using System.Collections;
using UnityEngine;

public class PlayerXP : MonoBehaviour
{
    [Header("XP Settings")]
    public int level = 1;

    public int currentXP = 0;
    public int xpToNextLevel = 15;
    public float xpGrowthFactor = 1.35f;

    [Header("References")]
    public UpgradeSelector upgradeUI;  
    public XPBarUI xpBarUI;            

    // Internal
    private int xpQueue = 0;           
    private bool isAnimatingXP = false;
    private int totalXP = 0;          
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
        UpdateXPUI();
    }

    private void OnEnemyDied(int xpGained)
    {
        GainXP(xpGained);
    }

    public void GainXP(int amount)
    {
        totalXP += amount;
        xpQueue += amount;

        if (!isAnimatingXP)
            StartCoroutine(ProcessXPQueue());
    }

    private IEnumerator ProcessXPQueue()
    {
        isAnimatingXP = true;

        while (xpQueue > 0)
        {
            int xpNeededThisLevel = xpToNextLevel - currentXP;
            int chunk = Mathf.Min(xpQueue, xpNeededThisLevel);

            xpQueue -= chunk;

            float startXP = currentXP;
            float endXP   = currentXP + chunk;

            float duration = 0.35f;
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                float lerpXP = Mathf.Lerp(startXP, endXP, t / duration);

                UpdateXPUI(lerpXP);

                yield return null;
            }

            currentXP += chunk;
            UpdateXPUI();

            if (currentXP >= xpToNextLevel)
            {
                currentXP -= xpToNextLevel;
                LevelUp();          
            }
        }

        isAnimatingXP = false;
    }

    private void LevelUp()
    {
        level++;
        Debug.Log($"LEVEL UP! New level: {level}");

        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * xpGrowthFactor);

        bool shouldGrantUpgrade = false;

        if (level <= 3)
        {
            shouldGrantUpgrade = true;
        }
        else if ((level % 2) == 1)
        {
            shouldGrantUpgrade = true;
        }

        if (shouldGrantUpgrade)
        {
            if (upgradeUI != null)
            {
                upgradeUI.QueueUpgrade();
            }
            else
            {
                Debug.LogWarning("PlayerXP: upgradeUI is null, cannot show upgrade choices.");
            }
        }

        UpdateXPUI();
    }

    private void UpdateXPUI()
    {
        if (xpBarUI != null)
            xpBarUI.SetXP(currentXP, xpToNextLevel, level);
    }

    private void UpdateXPUI(float currentXPOverride)
    {
        if (xpBarUI != null)
            xpBarUI.SetXP(currentXPOverride, xpToNextLevel, level);
    }
}
