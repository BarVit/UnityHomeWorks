using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillBook : MonoBehaviour
{
    private const int NoPrice = -1;
    private const float MinCooldown = 0.01f;

    [SerializeField] private Economy _economy;
    [SerializeField] private SkillTable _skills;
    [SerializeField] private WeaponTable _weapons;
    [SerializeField] private PlayerShip _ship;

    public event Action Changed;

    public int BaseHealth => _ship.BaseMaxHealth;

    public float GetStep(SkillKey skill)
    {
        return _skills.GetStep(skill.Weapon, skill.Stat);
    }

    public List<SkillKey> GetSkills()
    {
        List<SkillKey> skills = new List<SkillKey>();

        foreach (LevelRow<SkillStat> row in _skills.Rows)
        {
            SkillKey skill = new SkillKey(row.Stat, row.Owner);

            if (row.Level > 0 && skills.Contains(skill) == false)
                skills.Add(skill);
        }

        return skills;
    }

    public int GetLevel(SkillKey skill)
    {
        return _economy.Skills.Get(skill.StorageKey);
    }

    public int GetMaxLevel(SkillKey skill)
    {
        return _skills.GetMaxLevel(skill.Weapon, skill.Stat);
    }

    public float GetValue(SkillKey skill, int levelShift = 0)
    {
        return _skills.GetValue(skill.Weapon, skill.Stat, GetLevel(skill) + levelShift);
    }

    public int GetPrice(SkillKey skill)
    {
        LevelRow<SkillStat> next = _skills.GetRow(skill.Weapon, skill.Stat, GetLevel(skill) + 1);

        return next == null ? NoPrice : next.Price;
    }

    public float GetRateScale(string weaponId, int levelShift = 0)
    {
        WeaponRow weapon = _weapons.GetRow(weaponId);

        if (weapon == null || weapon.Rate <= 0f)
            return 1f;

        float bonus = GetValue(new SkillKey(SkillStat.Cooldown, weaponId), levelShift);

        return Mathf.Max(MinCooldown, weapon.Rate + bonus) / weapon.Rate;
    }

    public bool CanUpgrade(SkillKey skill)
    {
        int price = GetPrice(skill);

        return price != NoPrice && _economy.Xp >= price;
    }

    public bool TryUpgrade(SkillKey skill)
    {
        if (CanUpgrade(skill) == false || _economy.TrySpendXp(GetPrice(skill)) == false)
            return false;

        _economy.Skills.Raise(skill.StorageKey);
        Changed?.Invoke();

        return true;
    }

    public void Equip(PlayerShip ship)
    {
        ship.SetHealthBonus(Mathf.RoundToInt(GetValue(new SkillKey(SkillStat.Health, SkillTable.Everyone))));
    }
}
