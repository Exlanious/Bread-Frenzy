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

    [Header("Level Scaling")]
    public Transform playerModel;
    public float scalePerLevel = 0.15f;  
    public Vector3 baseScale = Vector3.one;
   

    // Internal
    private int xpQueue = 0;           
    private bool isAnimatingXP = false;
    private int totalXP = 0;          

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

        if (playerModel != null)
            baseScale = playerModel.localScale;
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
        ApplyLevelScaling();

        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * xpGrowthFactor);

        if (upgradeUI != null)
        {
            upgradeUI.QueueUpgrade();
        }
        else
        {
            Debug.LogWarning("PlayerXP: upgradeUI is null, cannot show upgrade choices.");
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

    private void ApplyLevelScaling()
    {
        if (playerModel == null) return;

        float scaleMultiplier = 1f + (scalePerLevel * (level - 1));

        Vector3 targetScale = baseScale * scaleMultiplier;

        StartCoroutine(AnimateScale(targetScale));
    }

    private IEnumerator AnimateScale(Vector3 target)
    {
        Vector3 start = playerModel.localScale;
        float t = 0f;
        float duration = 0.35f;

        while (t < duration)
        {
            t += Time.deltaTime;
            playerModel.localScale = Vector3.Lerp(start, target, t / duration);
            yield return null;
        }

        playerModel.localScale = target;
    }

}
