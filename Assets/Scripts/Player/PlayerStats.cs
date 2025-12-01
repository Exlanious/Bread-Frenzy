using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    public float baseDamage = 1f;
    public float baseRadius = 1f;

    public bool hasProjectile = false;
    public int projectileCount = 0;
    public float baseKnockback = 8f;  

    [Header("Current Final Stats (computed)")]
    public float damage;
    public float radius;
    public float knockback;

    public void RecomputeStats(
        float bonusDamageAdd,
        float bonusDamageMult,
        float bonusRadiusAdd,
        float bonusRadiusMult,
        bool unlockProj,
        int projCountIncrease,
        float bonusKnockbackAdd
    )
    {
        damage = (baseDamage + bonusDamageAdd) * bonusDamageMult;
        radius = (baseRadius + bonusRadiusAdd) * bonusRadiusMult;
        knockback = baseKnockback + bonusKnockbackAdd;

        if (unlockProj)
            hasProjectile = true;

        projectileCount += projCountIncrease;
    }

    void Start()
    {
        // Initialize with base values (no bonuses yet)
        RecomputeStats(
            bonusDamageAdd: 0f,
            bonusDamageMult: 1f,
            bonusRadiusAdd: 0f,
            bonusRadiusMult: 1f,
            unlockProj: false,
            projCountIncrease: 0,
            bonusKnockbackAdd: 0f
        );
    }
}
