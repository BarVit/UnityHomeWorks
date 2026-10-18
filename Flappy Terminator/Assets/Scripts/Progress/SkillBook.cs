using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillBook : MonoBehaviour
{
    private const int NoPrice = -1;
    private const float MinCooldown = 0.01f;
    private const float MinReloadTime = 0.1f;
    private const float PercentShare = 0.01f;

    [SerializeField] private Economy _economy;
    [SerializeField] private SkillTable _skills;
    [SerializeField] private WeaponTable _weapons;
    [SerializeField] private PlayerShip _ship;
    [SerializeField] private ModuleRack _modules;

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

            if (row.Level > 0 && skills.Contains(skill) == false && IsOpen(skill))
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

    public float GetReloadShift(string weaponId, int levelShift = 0)
    {
        return GetValue(new SkillKey(SkillStat.MagazineReload, weaponId), levelShift);
    }

    public float GetReloadTime(string weaponId, int levelShift = 0)
    {
        WeaponRow weapon = _weapons.GetRow(weaponId);

        return weapon == null ? 0f : Mathf.Max(MinReloadTime, weapon.ReloadTime + GetReloadShift(weaponId, levelShift));
    }

    public float GetShieldScale(int levelShift = 0)
    {
        return 1f + GetValue(new SkillKey(SkillStat.ShieldMastery, SkillTable.Everyone), levelShift) * PercentShare;
    }

    public float GetRepairScale(int levelShift = 0)
    {
        return 1f + GetValue(new SkillKey(SkillStat.RepairMastery, SkillTable.Everyone), levelShift) * PercentShare;
    }

    public bool IsOpen(SkillKey skill)
    {
        return skill.Stat != SkillStat.RepairMastery || _modules.IsKindOpen(ModuleKind.RepairDrone);
    }

    public bool CanUpgrade(SkillKey skill)
    {
        int price = GetPrice(skill);

        return IsOpen(skill) && price != NoPrice && _economy.Xp >= price;
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
