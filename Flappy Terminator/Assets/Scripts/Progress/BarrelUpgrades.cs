using System;
using UnityEngine;

[Serializable]
public class BarrelUpgrades
{
    [SerializeField] private int _damage;
    [SerializeField] private int _critChance;
    [SerializeField] private int _critDamage;
    [SerializeField] private int _heatReduction;
    [SerializeField] private int _doubleShot;
    [SerializeField] private int _projectileSpeed;
    [SerializeField] private int _napalm;

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
            case UpgradeStat.HeatReduction:
                return _heatReduction;
            case UpgradeStat.DoubleShot:
                return _doubleShot;
            case UpgradeStat.ProjectileSpeed:
                return _projectileSpeed;
            case UpgradeStat.Napalm:
                return _napalm;
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
            case UpgradeStat.HeatReduction:
                _heatReduction++;
                break;
            case UpgradeStat.DoubleShot:
                _doubleShot++;
                break;
            case UpgradeStat.ProjectileSpeed:
                _projectileSpeed++;
                break;
            case UpgradeStat.Napalm:
                _napalm++;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(stat), stat, null);
        }
    }
}
