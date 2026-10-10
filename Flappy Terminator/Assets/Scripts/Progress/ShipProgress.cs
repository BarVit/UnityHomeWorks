using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ShipProgress
{
    [SerializeField] private int _shipLevel = 1;
    [SerializeField] private int _shipXp;
    [SerializeField] private string _weaponId = string.Empty;
    [SerializeField] private List<WeaponLoadout> _loadouts = new List<WeaponLoadout>();

    public int ShipLevel => _shipLevel;

    public int ShipXp => _shipXp;

    public string WeaponId => _weaponId;

    public void AddXp(int amount)
    {
        _shipXp += amount;
    }

    public void SpendXp(int amount)
    {
        _shipXp -= amount;
        _shipLevel++;
    }

    public void SelectWeapon(string weaponId)
    {
        _weaponId = weaponId;
    }

    public WeaponLoadout GetLoadout(string weaponId)
    {
        foreach (WeaponLoadout loadout in _loadouts)
        {
            if (loadout.WeaponId == weaponId)
                return loadout;
        }

        WeaponLoadout created = new WeaponLoadout(weaponId);

        _loadouts.Add(created);

        return created;
    }
}
