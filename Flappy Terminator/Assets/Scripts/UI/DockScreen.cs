using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DockScreen : MonoBehaviour
{
    private const string Arrow = "  >  ";
    private const string PercentSign = "%";

    [SerializeField] private GameObject _panel;
    [SerializeField] private Hangar _hangar;
    [SerializeField] private Economy _economy;
    [SerializeField] private WeaponTable _weapons;
    [SerializeField] private TMP_Text _xp;
    [SerializeField] private TMP_Text _money;
    [SerializeField] private TMP_Text _earned;
    [SerializeField] private SlotView[] _slots;
    [SerializeField] private Image _weaponIcon;
    [SerializeField] private TMP_Text _weaponTitle;
    [SerializeField] private GameObject _upgradePanel;
    [SerializeField] private UpgradeRowView[] _upgrades;
    [SerializeField] private Button _menuButton;
    [SerializeField] private Button _nextButton;

    private int _selectedSlot = Hangar.FirstSlot;

    public event Action MenuRequested;
    public event Action NextRequested;

    private void OnEnable()
    {
        _menuButton.onClick.AddListener(OnMenuClicked);
        _nextButton.onClick.AddListener(OnNextClicked);
        _hangar.Changed += Refresh;
        _economy.Changed += Refresh;

        foreach (SlotView slot in _slots)
            slot.Clicked += OnSlotClicked;

        foreach (UpgradeRowView upgrade in _upgrades)
            upgrade.Requested += OnUpgradeRequested;
    }

    private void OnDisable()
    {
        _menuButton.onClick.RemoveListener(OnMenuClicked);
        _nextButton.onClick.RemoveListener(OnNextClicked);
        _hangar.Changed -= Refresh;
        _economy.Changed -= Refresh;

        foreach (SlotView slot in _slots)
            slot.Clicked -= OnSlotClicked;

        foreach (UpgradeRowView upgrade in _upgrades)
            upgrade.Requested -= OnUpgradeRequested;
    }

    public void Show(Reward earned, bool hasNextLocation)
    {
        _earned.text = "+" + earned.Xp + " XP     +" + earned.Money + " COINS";
        _nextButton.gameObject.SetActive(hasNextLocation);
        _selectedSlot = Hangar.FirstSlot;
        _panel.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        _panel.SetActive(false);
    }

    private void Refresh()
    {
        if (_panel.activeSelf == false)
            return;

        _xp.text = _economy.TotalXp.ToString();
        _money.text = _economy.Money.ToString();

        foreach (SlotView slot in _slots)
            ShowSlot(slot);

        ShowWeapon(_hangar.GetWeapon(_selectedSlot));
    }

    private void ShowSlot(SlotView slot)
    {
        if (_hangar.IsUnlocked(slot.Slot) == false)
        {
            slot.ShowLocked(_hangar.GetUnlockPrice(slot.Slot), _hangar.IsNextToUnlock(slot.Slot),
                _hangar.CanUnlock(slot.Slot));

            return;
        }

        WeaponRow row = _weapons.GetRow(_hangar.GetWeapon(slot.Slot));

        slot.ShowWeapon(GetIcon(row), row == null ? "EMPTY" : GetTitle(row), slot.Slot == _selectedSlot);
    }

    private void ShowWeapon(string weaponId)
    {
        WeaponRow row = _weapons.GetRow(weaponId);

        _upgradePanel.SetActive(row != null);

        if (row == null)
            return;

        _weaponIcon.sprite = GetIcon(row);
        _weaponTitle.text = GetTitle(row);

        foreach (UpgradeRowView upgrade in _upgrades)
            ShowUpgrade(upgrade, row);
    }

    private void ShowUpgrade(UpgradeRowView upgrade, WeaponRow row)
    {
        UpgradeStat stat = upgrade.Stat;
        int level = _hangar.GetLevel(row.Id, stat);
        int price = _hangar.GetUpgradePrice(row.Id, stat);
        string now = Format(row, stat, _hangar.GetValue(row.Id, stat));
        string value = price < 0 ? now : now + Arrow + Format(row, stat, _hangar.GetValue(row.Id, stat, 1));

        upgrade.Show(value, level, price, _hangar.CanUpgrade(row.Id, stat));
    }

    private string Format(WeaponRow row, UpgradeStat stat, float value)
    {
        switch (stat)
        {
            case UpgradeStat.Damage:
                return (row.Damage + Mathf.RoundToInt(value)).ToString();
            case UpgradeStat.CritChance:
                return Mathf.RoundToInt(value) + PercentSign;
            default:
                return "x" + value;
        }
    }

    private Sprite GetIcon(WeaponRow row)
    {
        return row == null || row.Assets == null ? null : row.Assets.Icon;
    }

    private string GetTitle(WeaponRow row)
    {
        return row.Id.Replace('_', ' ').ToUpperInvariant();
    }

    private void OnSlotClicked(int slot)
    {
        if (_hangar.IsUnlocked(slot))
        {
            _selectedSlot = slot;
            Refresh();

            return;
        }

        _hangar.TryUnlock(slot);
    }

    private void OnUpgradeRequested(UpgradeStat stat)
    {
        _hangar.TryUpgrade(_hangar.GetWeapon(_selectedSlot), stat);
    }

    private void OnMenuClicked()
    {
        MenuRequested?.Invoke();
    }

    private void OnNextClicked()
    {
        NextRequested?.Invoke();
    }
}
