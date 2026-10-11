using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModuleButtonView : MonoBehaviour
{
    [SerializeField] private int _slot;
    [SerializeField] private Button _button;
    [SerializeField] private Image _icon;
    [SerializeField] private Image _cooldown;
    [SerializeField] private GameObject _activeRing;
    [SerializeField] private TMP_Text _key;

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

    public void Show(Sprite icon, float readiness, bool isActive)
    {
        gameObject.SetActive(true);
        _icon.sprite = icon;
        _cooldown.fillAmount = 1f - readiness;
        _activeRing.SetActive(isActive);
        _key.text = (_slot + 1).ToString();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnClicked()
    {
        Clicked?.Invoke(_slot);
    }
}
