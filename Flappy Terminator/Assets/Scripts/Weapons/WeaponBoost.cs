using UnityEngine;

public readonly struct WeaponBoost
{
    public WeaponBoost(int damageBonus, float critChance, float critMultiplier)
    {
        DamageBonus = damageBonus;
        CritChance = critChance;
        CritMultiplier = critMultiplier;
    }

    public int DamageBonus { get; }

    public float CritChance { get; }

    public float CritMultiplier { get; }

    public bool RollCrit()
    {
        return CritChance > 0f && Random.value < CritChance;
    }

    public int ApplyCrit(int damage)
    {
        return Mathf.RoundToInt(damage * CritMultiplier);
    }

    public float ApplyCrit(float damage)
    {
        return damage * CritMultiplier;
    }
}
