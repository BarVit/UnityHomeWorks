using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ShipProgress
{
    [SerializeField] private int _shipLevel = 1;
    [SerializeField] private int _shipXp;
    [SerializeField] private List<string> _slotWeapons = new List<string>();
    [SerializeField] private List<WeaponUpgrades> _upgrades = new List<WeaponUpgrades>();

    public int ShipLevel => _shipLevel;

    public int ShipXp => _shipXp;

    public int UnlockedSlots => _slotWeapons.Count;

    public void AddXp(int amount)
    {
        _shipXp += amount;
    }

    public void SpendXp(int amount)
    {
        _shipXp -= amount;
        _shipLevel++;
    }

    public string GetSlotWeapon(int slot)
    {
        int index = slot - 1;

        return index >= 0 && index < _slotWeapons.Count ? _slotWeapons[index] : string.Empty;
    }

    public void UnlockSlot(string weaponId)
    {
        _slotWeapons.Add(weaponId ?? string.Empty);
    }

    public WeaponUpgrades GetUpgrades(string weaponId)
    {
        foreach (WeaponUpgrades upgrades in _upgrades)
        {
            if (upgrades.WeaponId == weaponId)
                return upgrades;
        }

        WeaponUpgrades created = new WeaponUpgrades(weaponId);

        _upgrades.Add(created);

        return created;
    }
}
