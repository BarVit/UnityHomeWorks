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

    public int Roll(int damage)
    {
        return IsCrit() ? Mathf.RoundToInt(damage * CritMultiplier) : damage;
    }

    public float Roll(float damage)
    {
        return IsCrit() ? damage * CritMultiplier : damage;
    }

    private bool IsCrit()
    {
        return CritChance > 0f && Random.value < CritChance;
    }
}
