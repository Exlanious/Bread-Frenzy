using UnityEngine;

public class PlayerRegeneration : MonoBehaviour
{
    public PlayerStats playerStats;
    public PlayerHealth playerHealth;
    public AbilityManager abilityManager;
    public AbilityUpgrade bakeryWarmthBase;
    public float noDamageDuration = 10f;
    public int instantHealAmount = 1;
    public float regenRateMultiplier = 1f;
    public float maxHealPerCycle = 5f;

    float timeSinceDamage;
    bool regenerating;
    int lastHealth;
    int lastMaxHealth;
    float regenBuffer;
    float healedThisCycle;

    void Awake()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();
        if (abilityManager == null)
            abilityManager = FindObjectOfType<AbilityManager>();

        if (playerHealth != null)
        {
            lastMaxHealth = playerHealth.maxHealth;
            lastHealth = playerHealth.maxHealth;
        }
    }

    void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged += OnHealthChanged;
    }

    void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= OnHealthChanged;
    }

    void OnHealthChanged(int current, int max)
    {
        if (current < lastHealth)
        {
            timeSinceDamage = 0f;
            regenerating = false;
            regenBuffer = 0f;
            healedThisCycle = 0f;
        }

        lastHealth = current;
        lastMaxHealth = max;
    }

    void Update()
    {
        if (playerHealth == null || playerStats == null || abilityManager == null || bakeryWarmthBase == null)
            return;

        if (!abilityManager.HasUpgrade(bakeryWarmthBase))
            return;

        if (lastHealth >= lastMaxHealth)
        {
            timeSinceDamage = 0f;
            regenerating = false;
            regenBuffer = 0f;
            healedThisCycle = 0f;
            return;
        }

        timeSinceDamage += Time.deltaTime;

        if (!regenerating && timeSinceDamage >= noDamageDuration)
        {
            regenerating = true;
            healedThisCycle = 0f;
            regenBuffer = 0f;

            if (instantHealAmount > 0)
                playerHealth.Heal(instantHealAmount);
        }

        if (!regenerating)
            return;

        if (playerStats.regen <= 0f)
            return;

        if (lastHealth >= lastMaxHealth)
            return;

        if (maxHealPerCycle > 0f && healedThisCycle >= maxHealPerCycle)
        {
            regenerating = false;
            regenBuffer = 0f;
            return;
        }

        float effectiveRegen = playerStats.regen * regenRateMultiplier;
        if (effectiveRegen <= 0f)
            return;

        regenBuffer += effectiveRegen * Time.deltaTime;

        if (regenBuffer >= 1f)
        {
            int healAmount = Mathf.FloorToInt(regenBuffer);
            regenBuffer -= healAmount;

            if (maxHealPerCycle > 0f)
            {
                float remaining = maxHealPerCycle - healedThisCycle;
                if (remaining <= 0f)
                {
                    regenerating = false;
                    regenBuffer = 0f;
                    return;
                }

                int clampedHeal = Mathf.Min(healAmount, Mathf.FloorToInt(remaining));
                if (clampedHeal <= 0)
                {
                    regenerating = false;
                    regenBuffer = 0f;
                    return;
                }

                healAmount = clampedHeal;
            }

            playerHealth.Heal(healAmount);
            healedThisCycle += healAmount;
        }
    }
}
