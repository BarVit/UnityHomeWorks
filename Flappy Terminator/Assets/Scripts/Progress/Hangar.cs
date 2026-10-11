using System;
using System.Collections.Generic;
using UnityEngine;

public class Hangar : MonoBehaviour
{
    public const int FirstSlot = 1;

    private const float PercentShare = 0.01f;
    private const int NoPrice = -1;

    [SerializeField] private Economy _economy;
    [SerializeField] private SlotTable _slots;
    [SerializeField] private UpgradeTable _upgrades;
    [SerializeField] private WeaponTable _weapons;
    [SerializeField] private int _testXp = 1000000;
    [SerializeField] private int _testMoney = 1000000;

    public event Action Changed;

    public string WeaponId => Ship.WeaponId;

    public int SlotCount
    {
        get
        {
            WeaponRow row = _weapons.GetRow(WeaponId);

            return row == null ? 0 : Mathf.Min(row.BarrelsMax, _slots.Count);
        }
    }

    private ShipProgress Ship => _economy.Ship;

    private WeaponLoadout Loadout => Ship.GetLoadout(WeaponId);

    private void Start()
    {
        StartNewSession();
    }

    public void StartNewSession()
    {
        Begin(_slots.GetRow(FirstSlot).Weapon, default);
    }

    public void StartTestSession(string weaponId)
    {
        Begin(weaponId, new Reward(_testXp, _testMoney));
    }

    public List<WeaponRow> GetShopWeapons()
    {
        List<WeaponRow> weapons = new List<WeaponRow>();

        foreach (WeaponRow row in _weapons.Rows)
        {
            if (row.Owner == WeaponOwner.Player)
                weapons.Add(row);
        }

        weapons.Sort((left, right) => left.Price.CompareTo(right.Price));

        return weapons;
    }

    public bool OwnsWeapon(string weaponId)
    {
        return Ship.OwnsWeapon(weaponId);
    }

    public bool CanBuyWeapon(string weaponId)
    {
        WeaponRow row = _weapons.GetRow(weaponId);

        return row != null && OwnsWeapon(weaponId) == false && _economy.Money >= row.Price;
    }

    public bool TryBuyWeapon(string weaponId)
    {
        if (CanBuyWeapon(weaponId) == false || _economy.TrySpend(_weapons.GetRow(weaponId).Price) == false)
            return false;

        Ship.AddWeapon(weaponId);
        Commit();

        return true;
    }

    public bool TrySelectWeapon(string weaponId)
    {
        if (OwnsWeapon(weaponId) == false)
            return false;

        Ship.SelectWeapon(weaponId);
        Commit();

        return true;
    }

    public List<UpgradeStat> GetUpgradeStats()
    {
        return _upgrades.GetUpgradableStats(WeaponId);
    }

    public WeaponRow GetWeapon()
    {
        return _weapons.GetRow(WeaponId);
    }

    public bool IsUnlocked(int slot)
    {
        return slot <= Loadout.BarrelCount;
    }

    public bool IsNextToUnlock(int slot)
    {
        return slot == Loadout.BarrelCount + 1;
    }

    public int GetUnlockPrice(int slot)
    {
        SlotRow row = _slots.GetRow(slot);

        return row == null ? NoPrice : row.Price;
    }

    public bool CanUnlock(int slot)
    {
        return slot <= SlotCount && IsNextToUnlock(slot) && GetUnlockPrice(slot) != NoPrice
            && _economy.Money >= GetUnlockPrice(slot);
    }

    public bool TryUnlock(int slot)
    {
        if (CanUnlock(slot) == false || _economy.TrySpend(GetUnlockPrice(slot)) == false)
            return false;

        Loadout.AddBarrel();
        Commit();

        return true;
    }

    public int GetLevel(int slot, UpgradeStat stat)
    {
        return GetBarrel(slot).GetLevel(stat);
    }

    public int GetMaxLevel(UpgradeStat stat)
    {
        return _upgrades.GetMaxLevel(WeaponId, stat);
    }

    public float GetStep(UpgradeStat stat)
    {
        return _upgrades.GetStep(WeaponId, stat);
    }

    public float GetValue(int slot, UpgradeStat stat, int levelShift = 0)
    {
        return _upgrades.GetValue(WeaponId, stat, GetLevel(slot, stat) + levelShift);
    }

    public int GetUpgradePrice(int slot, UpgradeStat stat)
    {
        LevelRow<UpgradeStat> next = _upgrades.GetRow(WeaponId, stat, GetLevel(slot, stat) + 1);

        return next == null ? NoPrice : next.Price;
    }

    public bool CanUpgrade(int slot, UpgradeStat stat)
    {
        int price = GetUpgradePrice(slot, stat);

        return IsUnlocked(slot) && price != NoPrice && _economy.Money >= price;
    }

    public bool TryUpgrade(int slot, UpgradeStat stat)
    {
        if (CanUpgrade(slot, stat) == false || _economy.TrySpend(GetUpgradePrice(slot, stat)) == false)
            return false;

        GetBarrel(slot).Raise(stat);

        Commit();

        return true;
    }

    public void Equip(PlayerShooter shooter, float rateScale)
    {
        WeaponBoost[] boosts = new WeaponBoost[Loadout.BarrelCount];

        for (int i = 0; i < boosts.Length; i++)
            boosts[i] = GetBoost(i + FirstSlot);

        shooter.Equip(WeaponId, boosts, rateScale);
    }

    private WeaponBoost GetBoost(int slot)
    {
        return new WeaponBoost(
            Mathf.RoundToInt(GetValue(slot, UpgradeStat.Damage)),
            GetValue(slot, UpgradeStat.CritChance) * PercentShare,
            GetValue(slot, UpgradeStat.CritDamage));
    }

    private BarrelUpgrades GetBarrel(int slot)
    {
        return Loadout.GetBarrel(slot - FirstSlot);
    }

    private void Begin(string weaponId, Reward grant)
    {
        _economy.StartNewSession();
        _economy.Grant(grant);
        Ship.SelectWeapon(weaponId);
        Changed?.Invoke();
    }

    private void Commit()
    {
        Changed?.Invoke();
    }
}
