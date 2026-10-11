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

    [SerializeField] private GameObject _panel;
    [SerializeField] private Hangar _hangar;
    [SerializeField] private ModuleRack _modules;
    [SerializeField] private Economy _economy;
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

            if (_hangar.OwnsWeapon(row.Id) == false)
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

            if (_modules.Owns(row.Id) == false)
                _cards[i].ShowPrice(row.Id, row.Icon, title, details, row.Price, _modules.CanBuy(row.Id));
            else if (_modules.IsInstalled(row.Id))
                _cards[i].ShowAction(row.Id, row.Icon, title, details, EquippedText, false, true);
            else if (_modules.CanInstall(row.Id))
                _cards[i].ShowAction(row.Id, row.Icon, title, details, EquipText, true, false);
            else
                _cards[i].ShowAction(row.Id, row.Icon, title, details, NoSlotText, false, false);
        }
    }

    private string GetModuleDetails(ModuleRow row)
    {
        switch (row.Kind)
        {
            case ModuleKind.Shield:
                return "ABSORBS " + Mathf.RoundToInt(_modules.GetValue(row.Id, ModuleStat.Capacity));
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
