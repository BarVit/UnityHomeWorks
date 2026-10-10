using UnityEngine;
using UnityEngine.EventSystems;

public class HoverHint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private NumberTooltip _tooltip;
    [SerializeField] private string _text;

    private void OnDisable()
    {
        _tooltip.Hide((RectTransform)transform);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _tooltip.Show(_text, (RectTransform)transform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _tooltip.Hide((RectTransform)transform);
    }
}
