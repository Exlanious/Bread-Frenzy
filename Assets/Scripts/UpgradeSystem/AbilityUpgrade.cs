using UnityEngine;

public enum StatType
{
    DamageAdd,
    DamageMultiplier,
    RadiusAdd,
    RadiusMultiplier,
    UnlockProjectile,
    ProjectileCountAdd
    // You can add MoveSpeed, AttackSpeed, etc later
}

[System.Serializable]
public class StatModifier
{
    public StatType statType;
    public float value;   // meaning depends on statType (add, mult, etc.)
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
