using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StarGateView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image _ring;
    [SerializeField] private RectTransform _vortex;
    [SerializeField] private GameObject _lock;
    [SerializeField] private TMP_Text _label;
    [SerializeField] private float _spinSpeed = -45f;
    [SerializeField] private Color _openColor = Color.white;
    [SerializeField] private Color _lockedColor = new Color(0.45f, 0.5f, 0.58f, 0.85f);

    public bool IsOpen { get; private set; }

    public event Action Clicked;

    private void Update()
    {
        if (IsOpen)
            _vortex.Rotate(0f, 0f, _spinSpeed * Time.unscaledDeltaTime);
    }

    public void Show(string label, bool isOpen)
    {
        IsOpen = isOpen;
        _label.text = label;
        _label.color = isOpen ? Color.white : _lockedColor;
        _ring.color = isOpen ? _openColor : _lockedColor;
        _vortex.gameObject.SetActive(isOpen);
        _lock.SetActive(isOpen == false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Clicked?.Invoke();
    }
}
