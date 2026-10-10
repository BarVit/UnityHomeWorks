using System;
using UnityEngine;

[Serializable]
public class WeaponUpgrades
{
    [SerializeField] private string _weaponId;
    [SerializeField] private int _damage;
    [SerializeField] private int _critChance;
    [SerializeField] private int _critDamage;

    public WeaponUpgrades(string weaponId)
    {
        _weaponId = weaponId;
    }

    public string WeaponId => _weaponId;

    public int GetLevel(UpgradeStat stat)
    {
        switch (stat)
        {
            case UpgradeStat.Damage:
                return _damage;
            case UpgradeStat.CritChance:
                return _critChance;
            case UpgradeStat.CritDamage:
                return _critDamage;
            default:
                throw new ArgumentOutOfRangeException(nameof(stat), stat, null);
        }
    }

    public void Raise(UpgradeStat stat)
    {
        switch (stat)
        {
            case UpgradeStat.Damage:
                _damage++;
                break;
            case UpgradeStat.CritChance:
                _critChance++;
                break;
            case UpgradeStat.CritDamage:
                _critDamage++;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(stat), stat, null);
        }
    }
}
