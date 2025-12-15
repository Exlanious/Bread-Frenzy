using UnityEngine;

public enum StatType
{
    DamageAdd,
    DamageMultiplier,
    RadiusAdd,
    RadiusMultiplier,
    UnlockProjectile,
    ProjectileCountAdd,
    KnockbackAdd,
    HpRegenAdd
}

[System.Serializable]
public class StatModifier
{
    public StatType statType;
    public float value;   
}

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Upgrades/AbilityUpgrade")]
public class AbilityUpgrade : ScriptableObject
{
    [Header("Info")]
    public string upgradeName;
    public string description;
    public Sprite icon;

    [Header("Modifiers (each stack applies all of these)")]
    public StatModifier[] modifiers;
}
