using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DockScreen : MonoBehaviour
{
    private const string Arrow = "  >  ";
    private const string PercentSign = "%";
    private const string BarrelMark = "  #";
    private const string ClearedStatus = "LOCATION CLEARED";
    private const string DestroyedStatus = "SHIP DESTROYED";
    private const string TestStatus = "TEST HANGAR";

    [SerializeField] private GameObject _panel;
    [SerializeField] private Hangar _hangar;
    [SerializeField] private Economy _economy;
    [SerializeField] private WeaponTable _weapons;
    [SerializeField] private CompactNumber _xp;
    [SerializeField] private CompactNumber _money;
    [SerializeField] private GameObject _earned;
    [SerializeField] private CompactNumber _earnedXp;
    [SerializeField] private CompactNumber _earnedMoney;
    [SerializeField] private TMP_Text _status;
    [SerializeField] private SlotView[] _slots;
    [SerializeField] private Image _weaponIcon;
    [SerializeField] private TMP_Text _weaponTitle;
    [SerializeField] private GameObject _upgradePanel;
    [SerializeField] private UpgradeRowView[] _upgrades;
    [SerializeField] private Button _menuButton;
    [FormerlySerializedAs("_nextButton")]
    [SerializeField] private Button _playButton;

    private int _selectedSlot = Hangar.FirstSlot;

    public event Action MenuRequested;
    public event Action PlayRequested;

    private void OnEnable()
    {
        _menuButton.onClick.AddListener(OnMenuClicked);
        _playButton.onClick.AddListener(OnPlayClicked);
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
        _playButton.onClick.RemoveListener(OnPlayClicked);
        _hangar.Changed -= Refresh;
        _economy.Changed -= Refresh;

        foreach (SlotView slot in _slots)
            slot.Clicked -= OnSlotClicked;

        foreach (UpgradeRowView upgrade in _upgrades)
            upgrade.Requested -= OnUpgradeRequested;
    }

    public void Show(Reward earned, bool canPlay, DockReason reason)
    {
        _earned.SetActive(reason != DockReason.TestStart);
        _earnedXp.Show(earned.Xp);
        _earnedMoney.Show(earned.Money);
        _status.text = GetStatus(reason);
        _playButton.gameObject.SetActive(canPlay);
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

        _xp.Show(_economy.TotalXp);
        _money.Show(_economy.Money);

        WeaponRow row = _weapons.GetRow(_hangar.WeaponId);

        if (_hangar.IsUnlocked(_selectedSlot) == false)
            _selectedSlot = Hangar.FirstSlot;

        foreach (SlotView slot in _slots)
            ShowSlot(slot, row);

        ShowWeapon(row);
    }

    private void ShowSlot(SlotView slot, WeaponRow row)
    {
        bool isAvailable = row != null && slot.Slot <= _hangar.SlotCount;

        slot.gameObject.SetActive(isAvailable);

        if (isAvailable == false)
            return;

        if (_hangar.IsUnlocked(slot.Slot) == false)
        {
            slot.ShowLocked(_hangar.GetUnlockPrice(slot.Slot), _hangar.IsNextToUnlock(slot.Slot),
                _hangar.CanUnlock(slot.Slot));

            return;
        }

        slot.ShowWeapon(GetIcon(row), GetTitle(row), slot.Slot == _selectedSlot);
    }

    private void ShowWeapon(WeaponRow row)
    {
        _upgradePanel.SetActive(row != null);

        if (row == null)
            return;

        _weaponIcon.sprite = GetIcon(row);
        _weaponTitle.text = _hangar.SlotCount > 1 ? GetTitle(row) + BarrelMark + _selectedSlot : GetTitle(row);

        foreach (UpgradeRowView upgrade in _upgrades)
            ShowUpgrade(upgrade, row);
    }

    private void ShowUpgrade(UpgradeRowView upgrade, WeaponRow row)
    {
        UpgradeStat stat = upgrade.Stat;
        int level = _hangar.GetLevel(_selectedSlot, stat);
        int price = _hangar.GetUpgradePrice(_selectedSlot, stat);
        string now = Format(row, stat, _hangar.GetValue(_selectedSlot, stat));
        string value = price < 0 ? now : now + Arrow + Format(row, stat, _hangar.GetValue(_selectedSlot, stat, 1));

        upgrade.Show(value, level, price, _hangar.CanUpgrade(_selectedSlot, stat));
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

    private string GetStatus(DockReason reason)
    {
        switch (reason)
        {
            case DockReason.Destroyed:
                return DestroyedStatus;
            case DockReason.TestStart:
                return TestStatus;
            default:
                return ClearedStatus;
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
        if (_hangar.IsUnlocked(slot) || _hangar.TryUnlock(slot))
        {
            _selectedSlot = slot;
            Refresh();
        }
    }

    private void OnUpgradeRequested(UpgradeStat stat)
    {
        _hangar.TryUpgrade(_selectedSlot, stat);
    }

    private void OnMenuClicked()
    {
        MenuRequested?.Invoke();
    }

    private void OnPlayClicked()
    {
        PlayRequested?.Invoke();
    }
}
