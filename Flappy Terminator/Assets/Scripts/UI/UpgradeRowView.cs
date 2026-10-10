using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeRowView : MonoBehaviour
{
    [SerializeField] private UpgradeStat _stat;
    [SerializeField] private TMP_Text _value;
    [SerializeField] private Image[] _pips;
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _price;
    [SerializeField] private Color _pipOn = Color.white;
    [SerializeField] private Color _pipOff = Color.white;

    public UpgradeStat Stat => _stat;

    public event Action<UpgradeStat> Requested;

    private void OnEnable()
    {
        _button.onClick.AddListener(OnClicked);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(OnClicked);
    }

    public void Show(string value, int level, int price, bool canBuy)
    {
        _value.text = value;

        for (int i = 0; i < _pips.Length; i++)
            _pips[i].color = i < level ? _pipOn : _pipOff;

        bool isMaxed = price < 0;

        _price.text = isMaxed ? "MAX" : price.ToString();
        _button.interactable = isMaxed == false && canBuy;
    }

    private void OnClicked()
    {
        Requested?.Invoke(_stat);
    }
}
