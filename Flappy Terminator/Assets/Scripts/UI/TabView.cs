using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TabView : MonoBehaviour
{
    [SerializeField] private int _index;
    [SerializeField] private Button _button;
    [SerializeField] private Image _background;
    [SerializeField] private TMP_Text _label;
    [SerializeField] private Color _selectedColor = Color.white;
    [SerializeField] private Color _idleColor = Color.gray;
    [SerializeField] private Color _selectedText = Color.white;
    [SerializeField] private Color _idleText = Color.gray;

    public int Index => _index;

    public event Action<int> Clicked;

    private void OnEnable()
    {
        _button.onClick.AddListener(OnClicked);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(OnClicked);
    }

    public void Show(bool isSelected)
    {
        _background.color = isSelected ? _selectedColor : _idleColor;
        _label.color = isSelected ? _selectedText : _idleText;
    }

    private void OnClicked()
    {
        Clicked?.Invoke(_index);
    }
}
