using UnityEngine;
using System.Collections.Generic;

public class AbilityManager : MonoBehaviour
{
    public PlayerStats stats;

    // Tracks how many times each upgrade has been taken
    private Dictionary<AbilityUpgrade, int> stacks = new Dictionary<AbilityUpgrade, int>();

    public void ApplyUpgrade(AbilityUpgrade upgrade)
    {
        if (!stacks.ContainsKey(upgrade))
            stacks.Add(upgrade, 0);

        stacks[upgrade]++;

        RecalculateAllStats();
    }

    private void RecalculateAllStats()
    {
        float totalDamageAdd = 0f;
        float totalDamageMult = 1f;

        float totalRadiusAdd = 0f;
        float totalRadiusMult = 1f;

        bool projectileUnlocked = false;
        int projectileCountAdd = 0;

        foreach (var kvp in stacks)
        {
            AbilityUpgrade upgrade = kvp.Key;
            int stackCount = kvp.Value;

            foreach (var mod in upgrade.modifiers)
            {
                switch (mod.statType)
                {
                    case StatType.DamageAdd:
                        totalDamageAdd += mod.value * stackCount;
                        break;

                    case StatType.DamageMultiplier:
                        // multipliers stack multiplicatively
                        totalDamageMult *= Mathf.Pow(mod.value, stackCount);
                        break;

                    case StatType.RadiusAdd:
                        totalRadiusAdd += mod.value * stackCount;
                        break;

                    case StatType.RadiusMultiplier:
                        totalRadiusMult *= Mathf.Pow(mod.value, stackCount);
                        break;

                    case StatType.UnlockProjectile:
                        if (stackCount > 0)
                            projectileUnlocked = true;
                        break;

                    case StatType.ProjectileCountAdd:
                        projectileCountAdd += Mathf.RoundToInt(mod.value * stackCount);
                        break;
                }
            }
        }

        stats.RecomputeStats(
            totalDamageAdd,
            totalDamageMult,
            totalRadiusAdd,
            totalRadiusMult,
            projectileUnlocked,
            projectileCountAdd
        );
    }
}
