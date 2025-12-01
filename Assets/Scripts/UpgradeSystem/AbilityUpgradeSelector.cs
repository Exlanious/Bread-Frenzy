using System.Collections.Generic;
using UnityEngine;

public class AbilityUpgradeSelector : MonoBehaviour
{
    public AbilityManager abilityManager;
    public AbilityUpgrade[] allUpgrades;
    public AbilityUpgrade toastedEdgeBase;   
    public AbilityUpgrade toastedEdgeStack; 


    void Awake()
    {
        if (abilityManager == null)
        {
            abilityManager = FindObjectOfType<AbilityManager>();
            if (abilityManager == null)
                Debug.LogError("AbilityUpgradeSelector: No AbilityManager found in scene.");
        }

        if (allUpgrades == null || allUpgrades.Length == 0)
        {
            allUpgrades = Resources.LoadAll<AbilityUpgrade>("Upgrades");

            if (allUpgrades.Length == 0)
                Debug.LogError("AbilityUpgradeSelector: No AbilityUpgrade assets found in Resources/Upgrades!");
        }
    }
    
    public AbilityUpgrade[] GetRandomOptions(int count)
    {
        if (allUpgrades == null || allUpgrades.Length == 0)
            return new AbilityUpgrade[0];

        bool hasToastedBase = false;
        if (abilityManager != null && toastedEdgeBase != null)
        {
            hasToastedBase = abilityManager.HasUpgrade(toastedEdgeBase);
        }

        List<AbilityUpgrade> pool = new List<AbilityUpgrade>();
        foreach (var upg in allUpgrades)
        {
            if (upg == null) continue;

            if (upg == toastedEdgeBase && hasToastedBase)
                continue;

            if (upg == toastedEdgeStack && !hasToastedBase)
                continue;

            pool.Add(upg);
        }

        if (pool.Count == 0)
            return new AbilityUpgrade[0];

        count = Mathf.Clamp(count, 0, pool.Count);

        AbilityUpgrade[] result = new AbilityUpgrade[count];

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, pool.Count);
            result[i] = pool[index];
            pool.RemoveAt(index);
        }

        return result;
    }
}
