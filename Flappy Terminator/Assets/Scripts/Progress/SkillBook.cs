using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillBook : MonoBehaviour
{
    private const int NoPrice = -1;
    private const float PercentShare = 0.01f;

    [SerializeField] private Economy _economy;
    [SerializeField] private SkillTable _skills;
    [SerializeField] private PlayerShip _ship;

    public event Action Changed;

    public int BaseHealth => _ship.BaseMaxHealth;

    public float GetStep(SkillStat skill)
    {
        return _skills.GetStep(SkillTable.Everyone, skill);
    }

    public List<SkillStat> GetSkills()
    {
        return _skills.GetUpgradableStats(SkillTable.Everyone);
    }

    public int GetLevel(SkillStat skill)
    {
        return _economy.Skills.Get(skill.ToString());
    }

    public int GetMaxLevel(SkillStat skill)
    {
        return _skills.GetMaxLevel(SkillTable.Everyone, skill);
    }

    public float GetValue(SkillStat skill, int levelShift = 0)
    {
        return _skills.GetValue(SkillTable.Everyone, skill, GetLevel(skill) + levelShift);
    }

    public int GetPrice(SkillStat skill)
    {
        LevelRow<SkillStat> next = _skills.GetRow(SkillTable.Everyone, skill, GetLevel(skill) + 1);

        return next == null ? NoPrice : next.Price;
    }

    public float GetRateScale(int levelShift = 0)
    {
        return 1f / (1f + GetValue(SkillStat.Cooldown, levelShift) * PercentShare);
    }

    public bool CanUpgrade(SkillStat skill)
    {
        int price = GetPrice(skill);

        return price != NoPrice && _economy.Xp >= price;
    }

    public bool TryUpgrade(SkillStat skill)
    {
        if (CanUpgrade(skill) == false || _economy.TrySpendXp(GetPrice(skill)) == false)
            return false;

        _economy.Skills.Raise(skill.ToString());
        Changed?.Invoke();

        return true;
    }

    public void Equip(PlayerShip ship)
    {
        ship.SetHealthBonus(Mathf.RoundToInt(GetValue(SkillStat.Health)));
    }
}
