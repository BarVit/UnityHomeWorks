using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ShipProgress
{
    [SerializeField] private string _weaponId = string.Empty;
    [SerializeField] private List<string> _ownedWeapons = new List<string>();
    [SerializeField] private List<WeaponLoadout> _loadouts = new List<WeaponLoadout>();
    [SerializeField] private List<string> _ownedModules = new List<string>();
    [SerializeField] private List<string> _installedModules = new List<string>();
    [SerializeField] private LevelBook _moduleLevels = new LevelBook();

    public string WeaponId => _weaponId;

    public IReadOnlyList<string> InstalledModules => _installedModules;

    public int ModuleSlots => _installedModules.Count;

    public LevelBook ModuleLevels => _moduleLevels;

    public bool OwnsWeapon(string weaponId)
    {
        return _ownedWeapons.Contains(weaponId);
    }

    public void AddWeapon(string weaponId)
    {
        if (OwnsWeapon(weaponId) == false)
            _ownedWeapons.Add(weaponId);
    }

    public void SelectWeapon(string weaponId)
    {
        AddWeapon(weaponId);
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

    public bool OwnsModule(string moduleId)
    {
        return _ownedModules.Contains(moduleId);
    }

    public void AddModule(string moduleId)
    {
        if (OwnsModule(moduleId) == false)
            _ownedModules.Add(moduleId);
    }

    public void AddModuleSlot()
    {
        _installedModules.Add(string.Empty);
    }

    public int FindModuleSlot(string moduleId)
    {
        return _installedModules.IndexOf(moduleId);
    }

    public void SetModule(int index, string moduleId)
    {
        _installedModules[index] = moduleId;
    }
}
