using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DockScreen : MonoBehaviour
{
    private const int WeaponTab = 0;
    private const int ModulesTab = 1;
    private const int SkillsTab = 2;
    private const string Arrow = "  >  ";
    private const string PercentSign = "%";
    private const string Seconds = " S";
    private const string BarrelMark = "  #";
    private const string EmptyCaption = "EMPTY";
    private const string MaxLevelHint = "MAX LEVEL";
    private const string EmptySlotHint = "EMPTY SLOT\nBUY A MODULE IN THE SHOP";
    private const string LockedSlotHint = "MODULE SLOT IS LOCKED\nUNLOCK IT TO INSTALL MODULES";
    private const string ClearedStatus = "LOCATION CLEARED";
    private const string DestroyedStatus = "SHIP DESTROYED";
    private const string TestStatus = "TEST HANGAR";

    [SerializeField] private GameObject _panel;
    [SerializeField] private Hangar _hangar;
    [SerializeField] private ModuleRack _modules;
    [SerializeField] private SkillBook _skills;
    [SerializeField] private Economy _economy;
    [SerializeField] private CompactNumber _xp;
    [SerializeField] private CompactNumber _money;
    [SerializeField] private GameObject _earned;
    [SerializeField] private CompactNumber _earnedXp;
    [SerializeField] private CompactNumber _earnedMoney;
    [SerializeField] private TMP_Text _status;
    [SerializeField] private TabView[] _tabs;
    [SerializeField] private GameObject[] _pages;
    [SerializeField] private SlotView[] _slots;
    [SerializeField] private Image _weaponIcon;
    [SerializeField] private TMP_Text _weaponTitle;
    [SerializeField] private GameObject _upgradePanel;
    [SerializeField] private UpgradeRowView[] _upgrades;
    [SerializeField] private SlotView[] _moduleSlots;
    [SerializeField] private GameObject _modulePanel;
    [SerializeField] private TMP_Text _moduleTitle;
    [SerializeField] private Button _removeModuleButton;
    [SerializeField] private UpgradeRowView[] _moduleUpgrades;
    [SerializeField] private TMP_Text _moduleHint;
    [SerializeField] private UpgradeRowView[] _skillRows;
    [SerializeField] private Sprite _coinIcon;
    [SerializeField] private Sprite _xpIcon;
    [SerializeField] private Sprite _damageIcon;
    [SerializeField] private Sprite _critIcon;
    [SerializeField] private Sprite _cooldownIcon;
    [SerializeField] private Sprite _healthIcon;
    [SerializeField] private ShopWindow _shop;
    [SerializeField] private Button _shopButton;
    [SerializeField] private Button _menuButton;
    [SerializeField] private Button _mapButton;
    [FormerlySerializedAs("_nextButton")]
    [SerializeField] private Button _playButton;

    private int _tab = WeaponTab;
    private int _selectedSlot = Hangar.FirstSlot;
    private int _selectedModuleSlot = ModuleRack.FirstSlot;
    private List<UpgradeStat> _weaponStats = new List<UpgradeStat>();
    private List<ModuleStat> _moduleStats = new List<ModuleStat>();
    private List<SkillKey> _skillStats = new List<SkillKey>();

    public bool CanPlay => _playButton.gameObject.activeSelf;

    public event Action MenuRequested;
    public event Action MapRequested;
    public event Action PlayRequested;

    private void OnEnable()
    {
        _menuButton.onClick.AddListener(OnMenuClicked);
        _mapButton.onClick.AddListener(OnMapClicked);
        _playButton.onClick.AddListener(OnPlayClicked);
        _shopButton.onClick.AddListener(OnShopClicked);
        _removeModuleButton.onClick.AddListener(OnRemoveModuleClicked);
        _hangar.Changed += Refresh;
        _modules.Changed += Refresh;
        _skills.Changed += Refresh;
        _economy.Changed += Refresh;

        foreach (TabView tab in _tabs)
            tab.Clicked += OnTabClicked;

        foreach (SlotView slot in _slots)
            slot.Clicked += OnSlotClicked;

        foreach (SlotView slot in _moduleSlots)
            slot.Clicked += OnModuleSlotClicked;

        foreach (UpgradeRowView upgrade in _upgrades)
            upgrade.Requested += OnUpgradeRequested;

        foreach (UpgradeRowView upgrade in _moduleUpgrades)
            upgrade.Requested += OnModuleUpgradeRequested;

        foreach (UpgradeRowView skill in _skillRows)
            skill.Requested += OnSkillRequested;
    }

    private void OnDisable()
    {
        _menuButton.onClick.RemoveListener(OnMenuClicked);
        _mapButton.onClick.RemoveListener(OnMapClicked);
        _playButton.onClick.RemoveListener(OnPlayClicked);
        _shopButton.onClick.RemoveListener(OnShopClicked);
        _removeModuleButton.onClick.RemoveListener(OnRemoveModuleClicked);
        _hangar.Changed -= Refresh;
        _modules.Changed -= Refresh;
        _skills.Changed -= Refresh;
        _economy.Changed -= Refresh;

        foreach (TabView tab in _tabs)
            tab.Clicked -= OnTabClicked;

        foreach (SlotView slot in _slots)
            slot.Clicked -= OnSlotClicked;

        foreach (SlotView slot in _moduleSlots)
            slot.Clicked -= OnModuleSlotClicked;

        foreach (UpgradeRowView upgrade in _upgrades)
            upgrade.Requested -= OnUpgradeRequested;

        foreach (UpgradeRowView upgrade in _moduleUpgrades)
            upgrade.Requested -= OnModuleUpgradeRequested;

        foreach (UpgradeRowView skill in _skillRows)
            skill.Requested -= OnSkillRequested;
    }

    public void Show(Reward earned, bool canPlay, DockReason reason)
    {
        _earned.SetActive(reason != DockReason.TestStart);
        _earnedXp.Show(earned.Xp);
        _earnedMoney.Show(earned.Money);
        _status.text = GetStatus(reason);
        _playButton.gameObject.SetActive(canPlay);
        _selectedSlot = Hangar.FirstSlot;
        _shop.Close();
        _panel.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        _shop.Close();
        _panel.SetActive(false);
    }

    public bool TryCloseShop()
    {
        if (_panel.activeSelf == false || _shop.IsOpen == false)
            return false;

        _shop.Close();

        return true;
    }

    private void Refresh()
    {
        if (_panel.activeSelf == false)
            return;

        _xp.Show(_economy.Xp);
        _money.Show(_economy.Money);

        for (int i = 0; i < _tabs.Length; i++)
        {
            _tabs[i].Show(_tabs[i].Index == _tab);
            _pages[i].SetActive(_tabs[i].Index == _tab);
        }

        if (_tab == WeaponTab)
            ShowWeaponPage();
        else if (_tab == ModulesTab)
            ShowModulePage();
        else
            ShowSkillPage();
    }

    private void ShowWeaponPage()
    {
        WeaponRow row = _hangar.GetWeapon();

        if (_hangar.IsUnlocked(_selectedSlot) == false)
            _selectedSlot = Hangar.FirstSlot;

        foreach (SlotView slot in _slots)
            ShowSlot(slot, row);

        _upgradePanel.SetActive(row != null);

        if (row == null)
            return;

        _weaponIcon.sprite = GetIcon(row);
        _weaponTitle.text = _hangar.SlotCount > 1 ? UiText.Title(row.Id) + BarrelMark + _selectedSlot : UiText.Title(row.Id);
        _weaponStats = _hangar.GetUpgradeStats();

        for (int i = 0; i < _upgrades.Length; i++)
        {
            if (i < _weaponStats.Count)
                _upgrades[i].Show(GetWeaponRow(row, _weaponStats[i]));
            else
                _upgrades[i].Hide();
        }
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

        slot.ShowWeapon(GetIcon(row), UiText.Title(row.Id), slot.Slot == _selectedSlot);
    }

    private UpgradeRowData GetWeaponRow(WeaponRow row, UpgradeStat stat)
    {
        int level = _hangar.GetLevel(_selectedSlot, stat);
        int price = _hangar.GetUpgradePrice(_selectedSlot, stat);
        string now = FormatWeapon(row, stat, _hangar.GetValue(_selectedSlot, stat));
        string value = price < 0 ? now : now + Arrow + FormatWeapon(row, stat, _hangar.GetValue(_selectedSlot, stat, 1));

        return new UpgradeRowData(GetWeaponIcon(stat), GetWeaponTitle(stat), GetWeaponHint(stat), value, level,
            _hangar.GetMaxLevel(stat), price, _hangar.CanUpgrade(_selectedSlot, stat), _coinIcon);
    }

    private string FormatWeapon(WeaponRow row, UpgradeStat stat, float value)
    {
        switch (stat)
        {
            case UpgradeStat.Damage:
                return (row.Damage + Mathf.RoundToInt(value)).ToString();
            case UpgradeStat.CritChance:
                return Mathf.RoundToInt(value) + PercentSign;
            case UpgradeStat.HeatReduction:
                return Mathf.RoundToInt(100f - value) + PercentSign;
            default:
                return "x" + value;
        }
    }

    private string GetWeaponTitle(UpgradeStat stat)
    {
        switch (stat)
        {
            case UpgradeStat.Damage:
                return "DAMAGE";
            case UpgradeStat.HeatReduction:
                return "HEAT PER SHOT";
            default:
                return "CRIT CHANCE";
        }
    }

    private string GetWeaponHint(UpgradeStat stat)
    {
        switch (stat)
        {
            case UpgradeStat.Damage:
                return "+" + _hangar.GetStep(stat) + " PER LEVEL";
            case UpgradeStat.HeatReduction:
                return "-" + _hangar.GetStep(stat) + PercentSign + " PER LEVEL";
            default:
                return "CRIT HIT DEALS x" + _hangar.GetValue(_selectedSlot, UpgradeStat.CritDamage);
        }
    }

    private Sprite GetWeaponIcon(UpgradeStat stat)
    {
        switch (stat)
        {
            case UpgradeStat.Damage:
                return _damageIcon;
            case UpgradeStat.HeatReduction:
                return _cooldownIcon;
            default:
                return _critIcon;
        }
    }

    private void ShowModulePage()
    {
        if (_modules.IsUnlocked(_selectedModuleSlot) == false)
            _selectedModuleSlot = ModuleRack.FirstSlot;

        foreach (SlotView slot in _moduleSlots)
            ShowModuleSlot(slot);

        ModuleRow module = _modules.GetInstalled(_selectedModuleSlot);

        _modulePanel.SetActive(module != null);
        _moduleHint.gameObject.SetActive(module == null);
        _moduleHint.text = _modules.IsUnlocked(_selectedModuleSlot) ? EmptySlotHint : LockedSlotHint;

        if (module == null)
            return;

        _moduleTitle.text = UiText.Title(module.Id);
        _moduleStats = _modules.GetUpgradeStats(module.Id);

        for (int i = 0; i < _moduleUpgrades.Length; i++)
        {
            if (i < _moduleStats.Count)
                _moduleUpgrades[i].Show(GetModuleRow(module, _moduleStats[i]));
            else
                _moduleUpgrades[i].Hide();
        }
    }

    private void ShowModuleSlot(SlotView slot)
    {
        bool isAvailable = slot.Slot <= _modules.SlotCount;

        slot.gameObject.SetActive(isAvailable);

        if (isAvailable == false)
            return;

        if (_modules.IsUnlocked(slot.Slot) == false)
        {
            slot.ShowLocked(_modules.GetUnlockPrice(slot.Slot), _modules.IsNextToUnlock(slot.Slot),
                _modules.CanUnlock(slot.Slot));

            return;
        }

        ModuleRow module = _modules.GetInstalled(slot.Slot);
        bool isSelected = slot.Slot == _selectedModuleSlot;

        if (module == null)
            slot.ShowEmpty(EmptyCaption, isSelected);
        else
            slot.ShowWeapon(module.Icon, UiText.Title(module.Id), isSelected);
    }

    private UpgradeRowData GetModuleRow(ModuleRow module, ModuleStat stat)
    {
        int price = _modules.GetUpgradePrice(module.Id, stat);
        string now = FormatModule(stat, _modules.GetValue(module.Id, stat));
        string value = price < 0 ? now : now + Arrow + FormatModule(stat, _modules.GetValue(module.Id, stat, 1));
        float step = _modules.GetValue(module.Id, stat, 1) - _modules.GetValue(module.Id, stat);
        string title = stat == ModuleStat.Capacity ? "SHIELD CAPACITY" : "COOLDOWN";
        string hint = price < 0 ? MaxLevelHint
            : (step > 0f ? "+" : "") + step + (stat == ModuleStat.Cooldown ? Seconds : "") + " NEXT LEVEL";
        Sprite icon = stat == ModuleStat.Capacity ? module.Icon : _cooldownIcon;

        return new UpgradeRowData(icon, title, hint, value, _modules.GetLevel(module.Id, stat),
            _modules.GetMaxLevel(module.Id, stat), price, _modules.CanUpgrade(module.Id, stat), _coinIcon);
    }

    private string FormatModule(ModuleStat stat, float value)
    {
        return stat == ModuleStat.Cooldown ? value + Seconds : Mathf.RoundToInt(value).ToString();
    }

    private void ShowSkillPage()
    {
        _skillStats = _skills.GetSkills();

        for (int i = 0; i < _skillRows.Length; i++)
        {
            if (i < _skillStats.Count)
                _skillRows[i].Show(GetSkillRow(_skillStats[i]));
            else
                _skillRows[i].Hide();
        }
    }

    private UpgradeRowData GetSkillRow(SkillKey skill)
    {
        int price = _skills.GetPrice(skill);
        string now = FormatSkill(skill, 0);
        string value = price < 0 ? now : now + Arrow + FormatSkill(skill, 1);
        bool isHull = skill.Stat == SkillStat.Health;
        string hint = isHull ? GetHullHint(skill, price)
            : "+" + _skills.GetStep(skill) + PercentSign + " FIRE RATE PER LEVEL";
        string title = isHull ? "HULL" : UiText.Title(skill.Weapon) + " COOLDOWN";

        return new UpgradeRowData(isHull ? _healthIcon : _cooldownIcon, title, hint, value,
            _skills.GetLevel(skill), _skills.GetMaxLevel(skill), price, _skills.CanUpgrade(skill), _xpIcon);
    }

    private string GetHullHint(SkillKey skill, int price)
    {
        if (price < 0)
            return MaxLevelHint;

        return "+" + Mathf.RoundToInt(_skills.GetValue(skill, 1) - _skills.GetValue(skill)) + " HEALTH NEXT LEVEL";
    }

    private string FormatSkill(SkillKey skill, int levelShift)
    {
        if (skill.Stat == SkillStat.Health)
            return (_skills.BaseHealth + Mathf.RoundToInt(_skills.GetValue(skill, levelShift))).ToString();

        WeaponRow row = _hangar.GetWeapon(skill.Weapon);
        float rate = row == null ? 0f : row.Rate;

        return (rate * _skills.GetRateScale(skill.Weapon, levelShift)).ToString("0.00") + Seconds;
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

    private void OnTabClicked(int tab)
    {
        _tab = tab;
        Refresh();
    }

    private void OnSlotClicked(int slot)
    {
        if (_hangar.IsUnlocked(slot) || _hangar.TryUnlock(slot))
        {
            _selectedSlot = slot;
            Refresh();
        }
    }

    private void OnModuleSlotClicked(int slot)
    {
        bool wasLocked = _modules.IsUnlocked(slot) == false;

        if (wasLocked && _modules.TryUnlock(slot) == false)
            return;

        _selectedModuleSlot = slot;

        if (_modules.GetInstalled(slot) == null)
        {
            string installable = _modules.FindInstallable();

            if (installable != null)
                _modules.TryInstall(installable);
            else if (wasLocked == false)
                _shop.Open(ShopWindow.ModulesTab);
        }

        Refresh();
    }

    private void OnRemoveModuleClicked()
    {
        _modules.Remove(_selectedModuleSlot);
    }

    private void OnUpgradeRequested(UpgradeRowView row)
    {
        int index = Array.IndexOf(_upgrades, row);

        if (index >= 0 && index < _weaponStats.Count)
            _hangar.TryUpgrade(_selectedSlot, _weaponStats[index]);
    }

    private void OnModuleUpgradeRequested(UpgradeRowView row)
    {
        int index = Array.IndexOf(_moduleUpgrades, row);
        ModuleRow module = _modules.GetInstalled(_selectedModuleSlot);

        if (module != null && index >= 0 && index < _moduleStats.Count)
            _modules.TryUpgrade(module.Id, _moduleStats[index]);
    }

    private void OnSkillRequested(UpgradeRowView row)
    {
        int index = Array.IndexOf(_skillRows, row);

        if (index >= 0 && index < _skillStats.Count)
            _skills.TryUpgrade(_skillStats[index]);
    }

    private void OnShopClicked()
    {
        _shop.Open(_tab == ModulesTab ? ShopWindow.ModulesTab : ShopWindow.WeaponsTab);
    }

    private void OnMenuClicked()
    {
        MenuRequested?.Invoke();
    }

    private void OnMapClicked()
    {
        MapRequested?.Invoke();
    }

    private void OnPlayClicked()
    {
        PlayRequested?.Invoke();
    }
}
