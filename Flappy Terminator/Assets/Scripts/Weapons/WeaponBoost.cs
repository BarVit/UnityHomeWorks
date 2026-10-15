using UnityEngine;

public readonly struct WeaponBoost
{
    public WeaponBoost(int damageBonus, float critChance, float critMultiplier, float heatReduction,
        float doubleShotChance)
    {
        DoubleShotChance = doubleShotChance;
        HeatReduction = heatReduction;
        DamageBonus = damageBonus;
        CritChance = critChance;
        CritMultiplier = critMultiplier;
    }

    public int DamageBonus { get; }

    public float CritChance { get; }

    public float CritMultiplier { get; }

    public float HeatReduction { get; }

    public float DoubleShotChance { get; }

    public float HeatScale => 1f - Mathf.Clamp01(HeatReduction);

    public bool RollCrit()
    {
        return CritChance > 0f && Random.value < CritChance;
    }

    public bool RollDoubleShot()
    {
        return DoubleShotChance > 0f && Random.value < DoubleShotChance;
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
