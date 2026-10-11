using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeRowView : MonoBehaviour
{
    private const float DimAlpha = 0.4f;
    private const string MaxText = "MAX";
    private const string LevelPrefix = "LV ";

    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _title;
    [SerializeField] private TMP_Text _hint;
    [SerializeField] private TMP_Text _value;
    [SerializeField] private Image[] _pips;
    [SerializeField] private TMP_Text _level;
    [SerializeField] private Button _button;
    [SerializeField] private CompactNumber _price;
    [SerializeField] private TMP_Text _priceLabel;
    [SerializeField] private Image _currency;
    [SerializeField] private Color _pipOn = Color.white;
    [SerializeField] private Color _pipOff = Color.white;

    public event Action<UpgradeRowView> Requested;

    private void OnEnable()
    {
        _button.onClick.AddListener(OnClicked);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(OnClicked);
    }

    public void Show(UpgradeRowData data)
    {
        gameObject.SetActive(true);
        _icon.sprite = data.Icon;
        _title.text = data.Title;
        _hint.text = data.Hint;
        _value.text = data.Value;
        _currency.sprite = data.Currency;
        ShowLevel(data.Level, data.MaxLevel);

        bool isMaxed = data.Price < 0;

        if (isMaxed)
            _price.ShowText(MaxText);
        else
            _price.Show(data.Price);

        _currency.enabled = isMaxed == false;
        _button.interactable = isMaxed == false && data.CanBuy;
        _priceLabel.alpha = _button.interactable ? 1f : DimAlpha;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void ShowLevel(int level, int maxLevel)
    {
        bool usesPips = maxLevel <= _pips.Length;

        for (int i = 0; i < _pips.Length; i++)
        {
            _pips[i].gameObject.SetActive(usesPips && i < maxLevel);
            _pips[i].color = i < level ? _pipOn : _pipOff;
        }

        _level.gameObject.SetActive(usesPips == false);
        _level.text = LevelPrefix + level + " / " + maxLevel;
    }

    private void OnClicked()
    {
        Requested?.Invoke(this);
    }
}
