using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopCardView : MonoBehaviour
{
    private const float DimAlpha = 0.4f;

    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _title;
    [SerializeField] private TMP_Text _details;
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _action;
    [SerializeField] private GameObject _priceGroup;
    [SerializeField] private CompactNumber _price;
    [SerializeField] private GameObject _highlight;

    public string Id { get; private set; }

    public event Action<ShopCardView> Clicked;

    private void OnEnable()
    {
        _button.onClick.AddListener(OnClicked);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(OnClicked);
    }

    public void ShowPrice(string id, Sprite icon, string title, string details, int price, bool canBuy)
    {
        Fill(id, icon, title, details, false);
        _action.gameObject.SetActive(false);
        _priceGroup.SetActive(true);
        _price.Show(price);
        SetButton(canBuy);
    }

    public void ShowAction(string id, Sprite icon, string title, string details, string action, bool canAct,
        bool isHighlighted)
    {
        Fill(id, icon, title, details, isHighlighted);
        _priceGroup.SetActive(false);
        _action.gameObject.SetActive(true);
        _action.text = action;
        SetButton(canAct);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Fill(string id, Sprite icon, string title, string details, bool isHighlighted)
    {
        Id = id;
        gameObject.SetActive(true);
        _icon.sprite = icon;
        _icon.enabled = icon != null;
        _title.text = title;
        _details.text = details;
        _highlight.SetActive(isHighlighted);
    }

    private void SetButton(bool isActive)
    {
        _button.interactable = isActive;

        foreach (TMP_Text text in _button.GetComponentsInChildren<TMP_Text>(true))
            text.alpha = isActive ? 1f : DimAlpha;
    }

    private void OnClicked()
    {
        Clicked?.Invoke(this);
    }
}
