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
    [SerializeField] private RectTransform _frame;
    [SerializeField] private float _frameSpinSpeed;
    [SerializeField] private Image _halo;
    [SerializeField] private float _haloPulseSpeed = 1.3f;
    [SerializeField] private float _haloMinAlpha = 0.35f;
    [SerializeField] private float _haloMaxAlpha = 1f;

    public bool IsOpen { get; private set; }

    public event Action Clicked;

    private void Update()
    {
        if (IsOpen == false)
            return;

        float deltaTime = Time.unscaledDeltaTime;

        _vortex.Rotate(0f, 0f, _spinSpeed * deltaTime);

        if (_frame != null)
            _frame.Rotate(0f, 0f, _frameSpinSpeed * deltaTime);

        if (_halo != null)
            Pulse();
    }

    public void Show(string label, bool isOpen)
    {
        IsOpen = isOpen;
        _label.text = label;
        _label.color = isOpen ? Color.white : _lockedColor;
        _ring.color = isOpen ? _openColor : _lockedColor;
        _vortex.gameObject.SetActive(isOpen);
        _lock.SetActive(isOpen == false);

        if (_halo != null)
            _halo.gameObject.SetActive(isOpen);
    }

    private void Pulse()
    {
        float wave = (Mathf.Sin(Time.unscaledTime * _haloPulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
        Color color = _halo.color;

        color.a = Mathf.Lerp(_haloMinAlpha, _haloMaxAlpha, wave);
        _halo.color = color;
        _halo.transform.localScale = Vector3.one * Mathf.Lerp(0.94f, 1.06f, wave);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Clicked?.Invoke();
    }
}
