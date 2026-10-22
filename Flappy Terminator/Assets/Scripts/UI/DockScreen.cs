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
    private const string PerSecond = "/S";
    private const string BarrelMark = "  #";
    private const string EmptyCaption = "EMPTY";
    private const string MaxLevelHint = "MAX LEVEL";
    private const string EmptySlotHint = "EMPTY SLOT\nBUY A MODULE IN THE SHOP";
    private const string LockedSlotHint = "MODULE SLOT IS LOCKED\nUNLOCK IT TO INSTALL MODULES";
    private const string ClearedStatus = "LOCATION CLEARED";
    private const string DestroyedStatus = "SHIP DESTROYED";
    private const string TestStatus = "TEST HANGAR";
    private const string ReceivedSeparator = "   ·   ";
    private const string ReceivedCaption = " RECEIVED";
    private const string SpentCaption = "SPENT\n";
    private const string CoinsCaption = " COINS\n";
    private const string XpCaption = " XP";

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
    [SerializeField] private Button _resetButton;
    [SerializeField] private TMP_Text _spent;
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
    [SerializeField] private RectTransform _skillContent;
    [SerializeField] private float _skillRowStep = 74f;
    [SerializeField] private Sprite _coinIcon;
    [SerializeField] private Sprite _xpIcon;
    [SerializeField] private Sprite _damageIcon;
    [SerializeField] private Sprite _critIcon;
    [SerializeField] private Sprite _doubleShotIcon;
    [SerializeField] private Sprite _speedIcon;
    [SerializeField] private Sprite _napalmIcon;
    [SerializeField] private Sprite _turnIcon;
    [SerializeField] private Sprite _reloadIcon;
    [SerializeField] private Sprite _magazineIcon;
    [SerializeField] private Sprite _shieldSkillIcon;
    [SerializeField] private Sprite _cooldownIcon;
    [SerializeField] private Sprite _healthIcon;
    [SerializeField] private Sprite _repairIcon;
    [SerializeField] private Sprite _attackTimeIcon;
    [SerializeField] private Sprite _lifetimeIcon;
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
    private readonly List<UpgradeRowView> _skillRowList = new List<UpgradeRowView>();

    public bool CanPlay => _playButton.gameObject.activeSelf;

    public event Action MenuRequested;
    public event Action MapRequested;
    public event Action PlayRequested;
    public event Action ResetRequested;

    private void Awake()
    {
        _skillRowList.AddRange(_skillRows);
    }

    private void OnEnable()
    {
        _menuButton.onClick.AddListener(OnMenuClicked);
        _mapButton.onClick.AddListener(OnMapClicked);
        _playButton.onClick.AddListener(OnPlayClicked);
        _shopButton.onClick.AddListener(OnShopClicked);
        _resetButton.onClick.AddListener(OnResetClicked);
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

        foreach (UpgradeRowView skill in _skillRowList)
            skill.Requested += OnSkillRequested;
    }

    private void OnDisable()
    {
        _menuButton.onClick.RemoveListener(OnMenuClicked);
        _mapButton.onClick.RemoveListener(OnMapClicked);
        _playButton.onClick.RemoveListener(OnPlayClicked);
        _shopButton.onClick.RemoveListener(OnShopClicked);
        _resetButton.onClick.RemoveListener(OnResetClicked);
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

        foreach (UpgradeRowView skill in _skillRowList)
            skill.Requested -= OnSkillRequested;
    }

    public void Show(Reward earned, bool canPlay, DockReason reason)
    {
        _earned.SetActive(reason != DockReason.TestStart);
        _earnedXp.Show(earned.Xp);
        _earnedMoney.Show(earned.Money);
        _status.text = GetStatus(reason) + GetReceived();
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
        _resetButton.gameObject.SetActive(_economy.IsTestSession);
        _spent.gameObject.SetActive(_economy.IsTestSession);
        _spent.text = SpentCaption + NumberFormat.Full(_economy.SpentMoney) + CoinsCaption
            + NumberFormat.Full(_economy.SpentXp) + XpCaption;

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

        return new UpgradeRowData(GetWeaponIcon(stat), GetWeaponTitle(stat), GetWeaponHint(stat, price), value, level,
            _hangar.GetMaxLevel(stat), price, _hangar.CanUpgrade(_selectedSlot, stat), _coinIcon);
    }

    private string FormatWeapon(WeaponRow row, UpgradeStat stat, float value)
    {
        switch (stat)
        {
            case UpgradeStat.Damage:
                return (row.Damage + Mathf.RoundToInt(value)).ToString();
            case UpgradeStat.CritChance:
            case UpgradeStat.DoubleShot:
            case UpgradeStat.Napalm:
                return Mathf.RoundToInt(value) + PercentSign;
            case UpgradeStat.ProjectileSpeed:
            case UpgradeStat.TurnRate:
                return Mathf.RoundToInt(100f + value) + PercentSign;
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
            case UpgradeStat.DoubleShot:
                return "DOUBLE SHOT";
            case UpgradeStat.ProjectileSpeed:
                return "FLIGHT SPEED";
            case UpgradeStat.Napalm:
                return "NAPALM";
            case UpgradeStat.TurnRate:
                return "TURN RATE";
            default:
                return "CRIT CHANCE";
        }
    }

    private string GetWeaponHint(UpgradeStat stat, int price)
    {
        switch (stat)
        {
            case UpgradeStat.Damage:
                return price < 0 ? MaxLevelHint
                    : "+" + Mathf.RoundToInt(_hangar.GetValue(_selectedSlot, stat, 1) - _hangar.GetValue(_selectedSlot, stat))
                    + " NEXT LEVEL";
            case UpgradeStat.HeatReduction:
                return "-" + _hangar.GetStep(stat) + PercentSign + " PER LEVEL";
            case UpgradeStat.DoubleShot:
                return "CHANCE OF A SECOND BULLET";
            case UpgradeStat.ProjectileSpeed:
                return "+" + _hangar.GetStep(stat) + PercentSign + " PER LEVEL";
            case UpgradeStat.Napalm:
                return "OF ROCKET DAMAGE AS 1.2 S BURN";
            case UpgradeStat.TurnRate:
                return "+" + _hangar.GetStep(stat) + PercentSign + " PER LEVEL";
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
            case UpgradeStat.DoubleShot:
                return _doubleShotIcon;
            case UpgradeStat.ProjectileSpeed:
                return _speedIcon;
            case UpgradeStat.Napalm:
                return _napalmIcon;
            case UpgradeStat.TurnRate:
                return _turnIcon;
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
            slot.ShowWeapon(_modules.GetIcon(module), UiText.Title(module.Id), isSelected);
    }

    private UpgradeRowData GetModuleRow(ModuleRow module, ModuleStat stat)
    {
        int price = _modules.GetUpgradePrice(module.Id, stat);
        float scale = _skills.GetModuleScale(stat);
        float current = _modules.GetFinalValue(module.Id, stat, scale);
        float next = _modules.GetFinalValue(module.Id, stat, scale, 1);
        string now = FormatModule(stat, current);
        string value = price < 0 ? now : now + Arrow + FormatModule(stat, next);
        float step = next - current;
        string title = GetModuleStatTitle(stat);
        string hint = price < 0 ? MaxLevelHint
            : (step > 0f ? "+" : "") + step.ToString("0.##") + (IsSeconds(stat) ? Seconds : "") + " NEXT LEVEL";
        Sprite icon = GetModuleStatIcon(module, stat);

        return new UpgradeRowData(icon, title, hint, value, _modules.GetLevel(module.Id, stat),
            _modules.GetMaxLevel(module.Id, stat), price, _modules.CanUpgrade(module.Id, stat), _coinIcon);
    }

    private string GetModuleStatTitle(ModuleStat stat)
    {
        switch (stat)
        {
            case ModuleStat.Capacity:
                return "SHIELD CAPACITY";
            case ModuleStat.Repair:
                return "REPAIR";
            case ModuleStat.Damage:
                return "DRONE DAMAGE";
            case ModuleStat.AttackTime:
                return "ATTACK TIME";
            case ModuleStat.Lifetime:
                return "DRONE LIFETIME";
            default:
                return "COOLDOWN";
        }
    }

    private Sprite GetModuleStatIcon(ModuleRow module, ModuleStat stat)
    {
        switch (stat)
        {
            case ModuleStat.Capacity:
                return _modules.GetIcon(module);
            case ModuleStat.Repair:
                return _repairIcon;
            case ModuleStat.Damage:
                return _damageIcon;
            case ModuleStat.AttackTime:
                return _attackTimeIcon;
            case ModuleStat.Lifetime:
                return _lifetimeIcon;
            default:
                return _cooldownIcon;
        }
    }

    private string FormatModule(ModuleStat stat, float value)
    {
        if (IsSeconds(stat))
            return value.ToString("0.#") + Seconds;

        return stat == ModuleStat.Damage ? value.ToString("0.#") + PerSecond : Mathf.RoundToInt(value).ToString();
    }

    private static bool IsSeconds(ModuleStat stat)
    {
        return stat == ModuleStat.Cooldown || stat == ModuleStat.AttackTime || stat == ModuleStat.Lifetime
            || stat == ModuleStat.Reload;
    }

    private void ShowSkillPage()
    {
        _skillStats = _skills.GetSkills();
        AddSkillRows(_skillStats.Count);

        for (int i = 0; i < _skillRowList.Count; i++)
        {
            if (i < _skillStats.Count)
                _skillRowList[i].Show(GetSkillRow(_skillStats[i]));
            else
                _skillRowList[i].Hide();
        }

        _skillContent.sizeDelta = new Vector2(_skillContent.sizeDelta.x, _skillStats.Count * _skillRowStep);
    }

    private void AddSkillRows(int count)
    {
        UpgradeRowView first = _skillRowList[0];
        Vector2 firstPosition = ((RectTransform)first.transform).anchoredPosition;

        while (_skillRowList.Count < count)
        {
            UpgradeRowView row = Instantiate(first, _skillContent);
            float offset = _skillRowStep * _skillRowList.Count;

            row.name = "Row " + (_skillRowList.Count + 1);
            ((RectTransform)row.transform).anchoredPosition = firstPosition + Vector2.down * offset;
            row.Requested += OnSkillRequested;
            _skillRowList.Add(row);
        }
    }

    private UpgradeRowData GetSkillRow(SkillKey skill)
    {
        int price = _skills.GetPrice(skill);
        string now = FormatSkill(skill, 0);
        string value = price < 0 ? now : now + Arrow + FormatSkill(skill, 1);
        return new UpgradeRowData(GetSkillIcon(skill), GetSkillTitle(skill), GetSkillHint(skill, price), value,
            _skills.GetLevel(skill), _skills.GetMaxLevel(skill), price, _skills.CanUpgrade(skill), _xpIcon);
    }

    private string GetSkillTitle(SkillKey skill)
    {
        switch (skill.Stat)
        {
            case SkillStat.Health:
                return "HULL";
            case SkillStat.MagazineReload:
                return UiText.Title(skill.Weapon) + " RELOAD";
            case SkillStat.MagazineSize:
                return UiText.Title(skill.Weapon) + " MAGAZINE";
            case SkillStat.FlightSpeed:
                return UiText.Title(skill.Weapon) + " FLIGHT SPEED";
            case SkillStat.ShieldMastery:
                return "SHIELD MASTERY";
            case SkillStat.RepairMastery:
                return "REPAIR MASTERY";
            default:
                return UiText.Title(skill.Weapon) + " COOLDOWN";
        }
    }

    private Sprite GetSkillIcon(SkillKey skill)
    {
        switch (skill.Stat)
        {
            case SkillStat.Health:
                return _healthIcon;
            case SkillStat.MagazineReload:
                return _reloadIcon;
            case SkillStat.MagazineSize:
                return _magazineIcon;
            case SkillStat.FlightSpeed:
                return _speedIcon;
            case SkillStat.ShieldMastery:
                return _shieldSkillIcon;
            case SkillStat.RepairMastery:
                return _repairIcon;
            default:
                return _cooldownIcon;
        }
    }

    private string GetSkillHint(SkillKey skill, int price)
    {
        if (price < 0)
            return MaxLevelHint;

        switch (skill.Stat)
        {
            case SkillStat.Health:
                return GetHullHint(skill, price);
            case SkillStat.ShieldMastery:
                return "+" + _skills.GetStep(skill) + PercentSign + " SHIELD PER LEVEL";
            case SkillStat.RepairMastery:
                return "+" + _skills.GetStep(skill) + PercentSign + " REPAIR PER LEVEL";
            case SkillStat.MagazineSize:
                return "+" + _skills.GetStep(skill) + " SHOTS PER LEVEL";
            case SkillStat.FlightSpeed:
                return "+" + _skills.GetStep(skill).ToString("0.###") + " SPEED PER LEVEL";
            default:
                return GetCooldownHint(skill, price);
        }
    }

    private string GetCooldownHint(SkillKey skill, int price)
    {
        if (price < 0)
            return MaxLevelHint;

        return _skills.GetStep(skill).ToString("0.###") + Seconds + " PER LEVEL";
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

        if (skill.Stat == SkillStat.ShieldMastery || skill.Stat == SkillStat.RepairMastery)
            return "+" + Mathf.RoundToInt(_skills.GetValue(skill, levelShift)) + PercentSign;

        if (skill.Stat == SkillStat.FlightSpeed)
            return _skills.GetFlightSpeed(skill.Weapon, levelShift).ToString("0.0");

        if (skill.Stat == SkillStat.MagazineSize)
            return _skills.GetMagazineSize(skill.Weapon, levelShift).ToString();

        if (skill.Stat == SkillStat.MagazineReload)
            return _skills.GetReloadTime(skill.Weapon, levelShift).ToString("0.00") + Seconds;

        WeaponRow row = _hangar.GetWeapon(skill.Weapon);
        float rate = row == null ? 0f : row.Rate;

        return (rate * _skills.GetRateScale(skill.Weapon, levelShift)).ToString("0.00") + Seconds;
    }

    private string GetReceived()
    {
        string received = string.Empty;

        foreach (string weaponId in _hangar.TakeGranted())
            received += ReceivedSeparator + UiText.Title(weaponId) + ReceivedCaption;

        return received;
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
        int index = _skillRowList.IndexOf(row);

        if (index >= 0 && index < _skillStats.Count)
            _skills.TryUpgrade(_skillStats[index]);
    }

    private void OnShopClicked()
    {
        _shop.Open(_tab == ModulesTab ? ShopWindow.ModulesTab : ShopWindow.WeaponsTab);
    }

    private void OnResetClicked()
    {
        ResetRequested?.Invoke();
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
