using System;
using System.Collections.Generic;
using UnityEngine;

public class ModuleRack : MonoBehaviour
{
    public const int FirstSlot = 1;

    private const int NoPrice = -1;

    [SerializeField] private Economy _economy;
    [SerializeField] private ModuleTable _modules;
    [SerializeField] private ModuleUpgradeTable _upgrades;
    [SerializeField] private SlotTable _slots;
    [SerializeField] private Sprite[] _shieldIcons;

    public event Action Changed;

    public int SlotCount => _slots.Count;

    public IReadOnlyList<ModuleRow> Modules => _modules.Rows;

    private ShipProgress Ship => _economy.Ship;

    public bool IsUnlocked(int slot)
    {
        return slot <= Ship.ModuleSlots;
    }

    public bool IsNextToUnlock(int slot)
    {
        return slot == Ship.ModuleSlots + 1;
    }

    public int GetUnlockPrice(int slot)
    {
        SlotRow row = _slots.GetRow(slot);

        return row == null ? NoPrice : row.Price;
    }

    public bool CanUnlock(int slot)
    {
        return IsNextToUnlock(slot) && GetUnlockPrice(slot) != NoPrice && _economy.Money >= GetUnlockPrice(slot);
    }

    public bool TryUnlock(int slot)
    {
        if (CanUnlock(slot) == false || _economy.TrySpend(GetUnlockPrice(slot)) == false)
            return false;

        Ship.AddModuleSlot();
        Commit();

        return true;
    }

    public ModuleRow GetInstalled(int slot)
    {
        if (IsUnlocked(slot) == false)
            return null;

        string id = Ship.InstalledModules[slot - FirstSlot];

        return id.Length == 0 ? null : _modules.GetRow(id);
    }

    public bool Owns(string moduleId)
    {
        return Ship.OwnsModule(moduleId);
    }

    public bool IsInstalled(string moduleId)
    {
        return Ship.FindModuleSlot(moduleId) >= 0;
    }

    public bool CanBuy(string moduleId)
    {
        ModuleRow row = _modules.GetRow(moduleId);

        return row != null && Owns(moduleId) == false && IsOpen(moduleId) && _economy.Money >= row.Price;
    }

    public bool IsOpen(string moduleId)
    {
        ModuleRow row = _modules.GetRow(moduleId);

        if (row == null)
            return false;

        if (_economy.IsTestSession || Owns(moduleId))
            return true;

        return _economy.IsRequirementMet(row.UnlockAfter);
    }

    public bool IsKindOpen(ModuleKind kind)
    {
        foreach (ModuleRow row in _modules.Rows)
        {
            if (row.Kind == kind && IsOpen(row.Id))
                return true;
        }

        return false;
    }

    public bool TryBuy(string moduleId)
    {
        if (CanBuy(moduleId) == false || _economy.TrySpend(_modules.GetRow(moduleId).Price) == false)
            return false;

        Ship.AddModule(moduleId);
        Commit();

        return true;
    }

    public bool CanInstall(string moduleId)
    {
        return Owns(moduleId) && IsInstalled(moduleId) == false && FindFreeSlot() >= 0;
    }

    public bool TryInstall(string moduleId)
    {
        if (CanInstall(moduleId) == false)
            return false;

        Install(moduleId);
        Commit();

        return true;
    }

    public string FindInstallable()
    {
        foreach (ModuleRow row in _modules.Rows)
        {
            if (CanInstall(row.Id))
                return row.Id;
        }

        return null;
    }

    public void Remove(int slot)
    {
        if (GetInstalled(slot) == null)
            return;

        Ship.SetModule(slot - FirstSlot, string.Empty);
        Commit();
    }

    public List<ModuleStat> GetUpgradeStats(string moduleId)
    {
        return _upgrades.GetUpgradableStats(moduleId);
    }

    public int GetLevel(string moduleId, ModuleStat stat)
    {
        return Ship.ModuleLevels.Get(GetKey(moduleId, stat));
    }

    public int GetMaxLevel(string moduleId, ModuleStat stat)
    {
        return _upgrades.GetMaxLevel(moduleId, stat);
    }

    public float GetValue(string moduleId, ModuleStat stat, int levelShift = 0)
    {
        return _upgrades.GetValue(moduleId, stat, GetLevel(moduleId, stat) + levelShift);
    }

    public int GetUpgradePrice(string moduleId, ModuleStat stat)
    {
        LevelRow<ModuleStat> next = _upgrades.GetRow(moduleId, stat, GetLevel(moduleId, stat) + 1);

        return next == null ? NoPrice : next.Price;
    }

    public bool CanUpgrade(string moduleId, ModuleStat stat)
    {
        int price = GetUpgradePrice(moduleId, stat);

        return Owns(moduleId) && price != NoPrice && _economy.Money >= price;
    }

    public bool TryUpgrade(string moduleId, ModuleStat stat)
    {
        if (CanUpgrade(moduleId, stat) == false || _economy.TrySpend(GetUpgradePrice(moduleId, stat)) == false)
            return false;

        Ship.ModuleLevels.Raise(GetKey(moduleId, stat));
        Commit();

        return true;
    }

    public int GetShieldLevel(string moduleId)
    {
        return Mathf.Min(ShieldColors.MaxLevel, GetLevel(moduleId, ModuleStat.Capacity));
    }

    public Sprite GetIcon(ModuleRow module)
    {
        if (module.Kind != ModuleKind.Shield || _shieldIcons == null || _shieldIcons.Length <= ShieldColors.MaxLevel)
            return module.Icon;

        return _shieldIcons[GetShieldLevel(module.Id)];
    }

    public void Equip(BattleModules battle, float shieldScale, float repairScale)
    {
        ModuleSpec?[] specs = new ModuleSpec?[SlotCount];

        for (int slot = FirstSlot; slot <= SlotCount; slot++)
        {
            ModuleRow row = GetInstalled(slot);

            if (row != null)
            {
                int capacity = Mathf.RoundToInt(GetValue(row.Id, ModuleStat.Capacity) * shieldScale);

                specs[slot - FirstSlot] = new ModuleSpec(row.Kind, GetIcon(row), capacity,
                    GetValue(row.Id, ModuleStat.Cooldown), GetShieldLevel(row.Id),
                    Mathf.RoundToInt(GetValue(row.Id, ModuleStat.Repair) * repairScale));
            }
        }

        battle.Load(specs);
    }

    private void Install(string moduleId)
    {
        int slot = FindFreeSlot();

        if (slot >= 0)
            Ship.SetModule(slot, moduleId);
    }

    private int FindFreeSlot()
    {
        return Ship.FindModuleSlot(string.Empty);
    }

    private static string GetKey(string moduleId, ModuleStat stat)
    {
        return moduleId + "/" + stat;
    }

    private void Commit()
    {
        Changed?.Invoke();
    }
}
