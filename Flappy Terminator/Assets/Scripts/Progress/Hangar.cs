using System;
using UnityEngine;

public class Hangar : MonoBehaviour
{
    public const int FirstSlot = 1;

    private const float PercentShare = 0.01f;
    private const int NoPrice = -1;

    [SerializeField] private Economy _economy;
    [SerializeField] private SlotTable _slots;
    [SerializeField] private UpgradeTable _upgrades;

    public int SlotCount => _slots.Count;

    public event Action Changed;

    private ShipProgress Ship => _economy.Ship;

    private void Start()
    {
        StartNewSession();
    }

    public void StartNewSession()
    {
        _economy.StartNewSession();
        Ship.UnlockSlot(_slots.GetRow(FirstSlot).Weapon);
        Changed?.Invoke();
    }

    public bool IsUnlocked(int slot)
    {
        return slot <= Ship.UnlockedSlots;
    }

    public bool IsNextToUnlock(int slot)
    {
        return slot == Ship.UnlockedSlots + 1;
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

        Ship.UnlockSlot(_slots.GetRow(slot).Weapon);
        Commit();

        return true;
    }

    public string GetWeapon(int slot)
    {
        return Ship.GetSlotWeapon(slot);
    }

    public int GetLevel(string weaponId, UpgradeStat stat)
    {
        return Ship.GetUpgrades(weaponId).GetLevel(stat);
    }

    public int GetMaxLevel(UpgradeStat stat)
    {
        return _upgrades.GetMaxLevel(stat);
    }

    public float GetValue(string weaponId, UpgradeStat stat, int levelShift = 0)
    {
        return _upgrades.GetValue(stat, GetLevel(weaponId, stat) + levelShift);
    }

    public int GetUpgradePrice(string weaponId, UpgradeStat stat)
    {
        UpgradeRow next = _upgrades.GetRow(stat, GetLevel(weaponId, stat) + 1);

        return next == null ? NoPrice : next.Price;
    }

    public bool CanUpgrade(string weaponId, UpgradeStat stat)
    {
        int price = GetUpgradePrice(weaponId, stat);

        return price != NoPrice && _economy.Money >= price;
    }

    public bool TryUpgrade(string weaponId, UpgradeStat stat)
    {
        if (CanUpgrade(weaponId, stat) == false || _economy.TrySpend(GetUpgradePrice(weaponId, stat)) == false)
            return false;

        Ship.GetUpgrades(weaponId).Raise(stat);
        Commit();

        return true;
    }

    public WeaponBoost GetBoost(string weaponId)
    {
        return new WeaponBoost(
            Mathf.RoundToInt(GetValue(weaponId, UpgradeStat.Damage)),
            GetValue(weaponId, UpgradeStat.CritChance) * PercentShare,
            GetValue(weaponId, UpgradeStat.CritDamage));
    }

    public void Equip(PlayerShooter shooter)
    {
        string weaponId = GetWeapon(FirstSlot);

        shooter.Equip(weaponId, GetBoost(weaponId));
    }

    private void Commit()
    {
        Changed?.Invoke();
    }
}
