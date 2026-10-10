using TMPro;
using UnityEngine;

public class NumberTooltip : MonoBehaviour
{
    [SerializeField] private RectTransform _panel;
    [SerializeField] private TMP_Text _label;
    [SerializeField] private float _padding = 14f;
    [SerializeField] private float _lift = 8f;

    private RectTransform _target;

    public void Show(string text, RectTransform target)
    {
        _target = target;
        _label.text = text;
        _panel.gameObject.SetActive(true);
        _panel.SetAsLastSibling();
        _panel.sizeDelta = new Vector2(_label.GetPreferredValues(text).x + _padding * 2f, _panel.sizeDelta.y);
        Place();
    }

    public void Hide(RectTransform target)
    {
        if (_target != target)
            return;

        _target = null;
        _panel.gameObject.SetActive(false);
    }

    private void Place()
    {
        Vector3[] target = new Vector3[4];
        Vector3[] screen = new Vector3[4];

        _target.GetWorldCorners(target);
        ((RectTransform)_panel.parent).GetWorldCorners(screen);

        float halfWidth = _panel.rect.width * 0.5f * _panel.lossyScale.x;
        float halfHeight = _panel.rect.height * 0.5f * _panel.lossyScale.y;
        float lift = _lift * _panel.lossyScale.y;
        float x = Mathf.Clamp((target[1].x + target[2].x) * 0.5f, screen[0].x + halfWidth, screen[2].x - halfWidth);
        float y = target[1].y + lift + halfHeight;

        if (y + halfHeight > screen[1].y)
            y = target[0].y - lift - halfHeight;

        _panel.position = new Vector3(x, y, _panel.position.z);
    }
}
