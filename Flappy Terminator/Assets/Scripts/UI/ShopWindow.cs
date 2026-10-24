using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopWindow : MonoBehaviour
{
    public const int WeaponsTab = 0;
    public const int ModulesTab = 1;

    private const string EquipText = "EQUIP";
    private const string EquippedText = "EQUIPPED";
    private const string NoSlotText = "NO FREE SLOT";
    private const string LockedText = "LOCKED";
    private const string ClearText = "CLEAR ";

    [SerializeField] private GameObject _panel;
    [SerializeField] private Hangar _hangar;
    [SerializeField] private ModuleRack _modules;
    [SerializeField] private SkillBook _skills;
    [SerializeField] private Economy _economy;
    [SerializeField] private LocationTable _locations;
    [SerializeField] private SystemTable _systems;
    [SerializeField] private Button _closeButton;
    [SerializeField] private TabView[] _tabs;
    [SerializeField] private ShopCardView[] _cards;

    private int _tab = WeaponsTab;

    public bool IsOpen => _panel.activeSelf;

    private void OnEnable()
    {
        _closeButton.onClick.AddListener(Close);
        _hangar.Changed += Refresh;
        _modules.Changed += Refresh;
        _economy.Changed += Refresh;

        foreach (TabView tab in _tabs)
            tab.Clicked += OnTabClicked;

        foreach (ShopCardView card in _cards)
            card.Clicked += OnCardClicked;
    }

    private void OnDisable()
    {
        _closeButton.onClick.RemoveListener(Close);
        _hangar.Changed -= Refresh;
        _modules.Changed -= Refresh;
        _economy.Changed -= Refresh;

        foreach (TabView tab in _tabs)
            tab.Clicked -= OnTabClicked;

        foreach (ShopCardView card in _cards)
            card.Clicked -= OnCardClicked;
    }

    public void Open(int tab)
    {
        _tab = tab;
        _panel.transform.SetAsLastSibling();
        _panel.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        _panel.SetActive(false);
    }

    private void Refresh()
    {
        if (_panel.activeSelf == false)
            return;

        foreach (TabView tab in _tabs)
            tab.Show(tab.Index == _tab);

        if (_tab == WeaponsTab)
            ShowWeapons();
        else
            ShowModules();
    }

    private void ShowWeapons()
    {
        List<WeaponRow> weapons = _hangar.GetShopWeapons();

        for (int i = 0; i < _cards.Length; i++)
        {
            if (i >= weapons.Count)
            {
                _cards[i].Hide();

                continue;
            }

            WeaponRow row = weapons[i];
            Sprite icon = row.Assets == null ? null : row.Assets.Icon;
            string title = UiText.Title(row.Id);
            string details = "DAMAGE " + row.Damage;

            if (_hangar.IsWeaponOpen(row.Id) == false)
                _cards[i].ShowAction(row.Id, icon, title, details, GetLockText(GetRequirement(row)), false, false);
            else if (_hangar.OwnsWeapon(row.Id) == false)
                _cards[i].ShowPrice(row.Id, icon, title, details, row.Price, _hangar.CanBuyWeapon(row.Id));
            else if (row.Id == _hangar.WeaponId)
                _cards[i].ShowAction(row.Id, icon, title, details, EquippedText, false, true);
            else
                _cards[i].ShowAction(row.Id, icon, title, details, EquipText, true, false);
        }
    }

    private void ShowModules()
    {
        IReadOnlyList<ModuleRow> modules = _modules.Modules;

        for (int i = 0; i < _cards.Length; i++)
        {
            if (i >= modules.Count)
            {
                _cards[i].Hide();

                continue;
            }

            ModuleRow row = modules[i];
            string title = UiText.Title(row.Id);
            string details = GetModuleDetails(row);

            if (_modules.IsOpen(row.Id) == false)
                _cards[i].ShowAction(row.Id, row.Icon, title, details, GetLockText(row.UnlockAfter), false, false);
            else if (_modules.Owns(row.Id) == false)
                _cards[i].ShowPrice(row.Id, row.Icon, title, details, row.Price, _modules.CanBuy(row.Id));
            else if (_modules.IsInstalled(row.Id))
                _cards[i].ShowAction(row.Id, row.Icon, title, details, EquippedText, false, true);
            else if (_modules.CanInstall(row.Id))
                _cards[i].ShowAction(row.Id, row.Icon, title, details, EquipText, true, false);
            else
                _cards[i].ShowAction(row.Id, row.Icon, title, details, NoSlotText, false, false);
        }
    }

    private string GetRequirement(WeaponRow row)
    {
        return string.IsNullOrEmpty(row.UnlockAfter) ? row.GrantedAfter : row.UnlockAfter;
    }

    private string GetLockText(string requirement)
    {
        if (string.IsNullOrEmpty(requirement))
            return LockedText;

        if (_locations.IsVoid(requirement))
            return ClearText + UiText.Title(requirement);

        LocationRow location = _locations.GetRow(requirement);

        if (location == null)
            return LockedText;

        if (location.IsBranch)
            return ClearText + location.Title;

        SystemRow system = _systems.GetRow(location.System);

        return ClearText + (system == null ? location.Id : system.Order + "-" + location.Order);
    }

    private float GetFinalValue(ModuleRow row, ModuleStat stat)
    {
        return _modules.GetFinalValue(row.Id, stat, _skills.GetModuleScale(stat));
    }

    private string GetModuleDetails(ModuleRow row)
    {
        switch (row.Kind)
        {
            case ModuleKind.Shield:
                return "ABSORBS " + GetFinalValue(row, ModuleStat.Capacity);
            case ModuleKind.RepairDrone:
                return "REPAIRS " + GetFinalValue(row, ModuleStat.Repair) + " HP";
            case ModuleKind.RepairNanobots:
                return "REPAIRS " + GetFinalValue(row, ModuleStat.Regen) + " HP/S";
            case ModuleKind.SentryDrones:
                return "2 DRONES, " + GetFinalValue(row, ModuleStat.Damage) + " DMG/S EACH";
            default:
                return row.Description;
        }
    }

    private void OnTabClicked(int tab)
    {
        _tab = tab;
        Refresh();
    }

    private void OnCardClicked(ShopCardView card)
    {
        if (_tab == WeaponsTab)
        {
            if (_hangar.OwnsWeapon(card.Id) == false)
                _hangar.TryBuyWeapon(card.Id);
            else if (_hangar.TrySelectWeapon(card.Id))
                Close();

            return;
        }

        if (_modules.Owns(card.Id) == false)
            _modules.TryBuy(card.Id);
        else if (_modules.TryInstall(card.Id))
            Close();
    }
}
