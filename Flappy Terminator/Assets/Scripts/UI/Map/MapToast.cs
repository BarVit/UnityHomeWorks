using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class MapToast : MonoBehaviour
{
    [SerializeField] private TMP_Text _label;
    [SerializeField] private float _fadeIn = 0.15f;
    [SerializeField] private float _hold = 1.2f;
    [SerializeField] private float _fadeOut = 0.6f;

    private CanvasGroup _group;
    private float _age = float.MaxValue;

    private void Awake()
    {
        _group = GetComponent<CanvasGroup>();
        _group.alpha = 0f;
    }

    private void Update()
    {
        _age += Time.unscaledDeltaTime;

        if (_age < _fadeIn)
            _group.alpha = _age / _fadeIn;
        else if (_age < _fadeIn + _hold)
            _group.alpha = 1f;
        else
            _group.alpha = Mathf.Clamp01(1f - (_age - _fadeIn - _hold) / _fadeOut);
    }

    private void OnDisable()
    {
        _age = float.MaxValue;

        if (_group != null)
            _group.alpha = 0f;
    }

    public void Show(string text)
    {
        _label.text = text;
        _age = 0f;
    }
}
