using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CompactNumber : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private NumberTooltip _tooltip;
    [SerializeField] private string _prefix = string.Empty;
    [SerializeField] private string _suffix = string.Empty;

    private TMP_Text _label;
    private long _value;
    private bool _hasValue;

    private TMP_Text Label => _label != null ? _label : _label = GetComponent<TMP_Text>();

    private void OnDisable()
    {
        HideTooltip();
    }

    public void Show(long value)
    {
        _value = value;
        _hasValue = true;
        Label.text = _prefix + NumberFormat.Compact(value) + _suffix;
    }

    public void ShowText(string text)
    {
        _hasValue = false;
        Label.text = text;
        HideTooltip();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_hasValue && _tooltip != null)
            _tooltip.Show(_prefix + NumberFormat.Full(_value) + _suffix, (RectTransform)transform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    private void HideTooltip()
    {
        if (_tooltip != null)
            _tooltip.Hide((RectTransform)transform);
    }
}
