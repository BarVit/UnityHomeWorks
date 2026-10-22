using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SentryTracer : MonoBehaviour
{
    private const int Points = 2;

    [SerializeField] private float _duration = 0.12f;
    [SerializeField] private Color _color = new Color(0.78f, 0.78f, 0.8f, 1f);
    [SerializeField, Range(0f, 1f)] private float _tailAlpha = 0f;
    [SerializeField, Range(0f, 1f)] private float _headAlpha = 0.85f;

    private LineRenderer _line;
    private float _left;

    public bool IsBusy => gameObject.activeSelf;

    private void Awake()
    {
        _line = GetComponent<LineRenderer>();
        _line.positionCount = Points;
        _line.useWorldSpace = true;
    }

    private void Update()
    {
        _left -= Time.deltaTime;

        if (_left <= 0f)
        {
            gameObject.SetActive(false);

            return;
        }

        Paint(_left / _duration);
    }

    public void Show(Vector2 from, Vector2 to)
    {
        gameObject.SetActive(true);
        _line.SetPosition(0, from);
        _line.SetPosition(1, to);
        _left = _duration;
        Paint(1f);
    }

    private void Paint(float share)
    {
        Color tail = new Color(_color.r, _color.g, _color.b, _color.a * _tailAlpha * share);
        Color head = new Color(_color.r, _color.g, _color.b, _color.a * _headAlpha * share);

        _line.startColor = tail;
        _line.endColor = head;
    }
}
