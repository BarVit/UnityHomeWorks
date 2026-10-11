using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WeaponLoadout
{
    [SerializeField] private string _weaponId;
    [SerializeField] private List<BarrelUpgrades> _barrels = new List<BarrelUpgrades>();
    [SerializeField] private LevelBook _weaponLevels = new LevelBook();

    public WeaponLoadout(string weaponId)
    {
        _weaponId = weaponId;
        AddBarrel();
    }

    public string WeaponId => _weaponId;

    public int BarrelCount => _barrels.Count;

    public LevelBook WeaponLevels => _weaponLevels;

    public BarrelUpgrades GetBarrel(int barrel)
    {
        return _barrels[barrel];
    }

    public void AddBarrel()
    {
        _barrels.Add(new BarrelUpgrades());
    }
}
