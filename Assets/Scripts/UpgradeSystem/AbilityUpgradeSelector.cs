using System.Collections.Generic;
using UnityEngine;

public class AbilityUpgradeSelector : MonoBehaviour
{
    public AbilityManager abilityManager;
    public AbilityUpgrade[] allUpgrades;

    void Awake()
    {
        // Auto-find AbilityManager
        if (abilityManager == null)
        {
            abilityManager = FindObjectOfType<AbilityManager>();
            if (abilityManager == null)
                Debug.LogError("AbilityUpgradeSelector: No AbilityManager found in scene.");
        }

        // Auto-load all upgrades from Resources folder
        if (allUpgrades == null || allUpgrades.Length == 0)
        {
            allUpgrades = Resources.LoadAll<AbilityUpgrade>("Upgrades");

            if (allUpgrades.Length == 0)
                Debug.LogError("AbilityUpgradeSelector: No AbilityUpgrade assets found in Resources/Upgrades!");
        }
    }

    /// <summary>
    /// Returns a set of unique random upgrades from the full pool.
    /// </summary>
    public AbilityUpgrade[] GetRandomOptions(int count)
    {
        if (allUpgrades == null || allUpgrades.Length == 0)
            return new AbilityUpgrade[0];

        count = Mathf.Min(count, allUpgrades.Length);

        List<AbilityUpgrade> pool = new List<AbilityUpgrade>(allUpgrades);
        AbilityUpgrade[] result = new AbilityUpgrade[count];

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, pool.Count);
            result[i] = pool[index];
            pool.RemoveAt(index); // ensure no duplicates
        }

        return result;
    }
}
