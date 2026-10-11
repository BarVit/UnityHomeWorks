using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotView : MonoBehaviour
{
    [SerializeField] private int _slot;
    [SerializeField] private Button _button;
    [SerializeField] private Image _icon;
    [SerializeField] private GameObject _lock;
    [SerializeField] private GameObject _priceGroup;
    [SerializeField] private CompactNumber _price;
    [SerializeField] private TMP_Text _caption;
    [SerializeField] private GameObject _selection;
    [SerializeField] private CanvasGroup _group;

    private const float DimAlpha = 0.45f;

    public int Slot => _slot;

    public event Action<int> Clicked;

    private void OnEnable()
    {
        _button.onClick.AddListener(OnClicked);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(OnClicked);
    }

    public void ShowWeapon(Sprite icon, string title, bool isSelected)
    {
        _lock.SetActive(false);
        _priceGroup.SetActive(false);
        _icon.gameObject.SetActive(icon != null);
        _icon.sprite = icon;
        _caption.text = title;
        _selection.SetActive(isSelected);
        _button.interactable = true;
        _group.alpha = 1f;
    }

    public void ShowEmpty(string caption, bool isSelected)
    {
        _lock.SetActive(false);
        _priceGroup.SetActive(false);
        _icon.gameObject.SetActive(false);
        _caption.text = caption;
        _selection.SetActive(isSelected);
        _button.interactable = true;
        _group.alpha = 1f;
    }

    public void ShowLocked(int price, bool isNext, bool canAfford)
    {
        _lock.SetActive(true);
        _icon.gameObject.SetActive(false);
        _priceGroup.SetActive(true);
        _price.Show(price);
        _caption.text = string.Empty;
        _selection.SetActive(false);
        _button.interactable = isNext && canAfford;
        _group.alpha = isNext ? 1f : DimAlpha;
    }

    private void OnClicked()
    {
        Clicked?.Invoke(_slot);
    }
}
