using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class EmpBolt : MonoBehaviour
{
    [SerializeField] private float _duration = 0.18f;
    [SerializeField] private Color _color = new Color(0.85f, 0.97f, 1f, 1f);
    [SerializeField] private Color _glowColor = new Color(0.35f, 0.7f, 1f, 0.45f);
    [SerializeField] private LineRenderer _glow;

    private LineRenderer _line;
    private float _left;

    public bool IsBusy => gameObject.activeSelf;

    private void Awake()
    {
        _line = GetComponent<LineRenderer>();
        _line.useWorldSpace = true;

        if (_glow != null)
            _glow.useWorldSpace = true;
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

    public void Show(Vector3[] points, int count)
    {
        gameObject.SetActive(true);
        _line.positionCount = count;
        _line.SetPositions(points);

        if (_glow != null)
        {
            _glow.positionCount = count;
            _glow.SetPositions(points);
        }

        _left = _duration;
        Paint(1f);
    }

    private void Paint(float share)
    {
        Color color = new Color(_color.r, _color.g, _color.b, _color.a * share);

        _line.startColor = color;
        _line.endColor = color;

        if (_glow == null)
            return;

        Color glow = new Color(_glowColor.r, _glowColor.g, _glowColor.b, _glowColor.a * share);

        _glow.startColor = glow;
        _glow.endColor = glow;
    }
}
