using UnityEngine;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Health _health;
    [SerializeField] private SpriteRenderer _body;
    [SerializeField] private Transform _bar;
    [SerializeField] private Transform _fill;
    [SerializeField] private Transform _trail;
    [SerializeField] private float _widthShare = 0.8f;
    [SerializeField] private float _thickness = 0.06f;
    [SerializeField] private float _gap = 0.08f;
    [SerializeField] private float _trailHoldTime = 0.25f;
    [SerializeField] private float _trailFallTime = 0.35f;

    private float _fillPart = 1f;
    private float _trailPart = 1f;
    private float _trailFrom = 1f;
    private float _trailTimer;

    private void OnEnable()
    {
        _health.Changed += OnHealthChanged;
    }

    private void OnDisable()
    {
        _health.Changed -= OnHealthChanged;
    }

    private void Update()
    {
        if (_trailPart <= _fillPart)
            return;

        _trailTimer += Time.deltaTime;

        if (_trailTimer < _trailHoldTime)
            return;

        float share = (_trailTimer - _trailHoldTime) / _trailFallTime;

        _trailPart = Mathf.Lerp(_trailFrom, _fillPart, share);
        Show(_trail, _trailPart);
    }

    private void OnHealthChanged(int value)
    {
        float part = _health.MaxValue > 0 ? (float)value / _health.MaxValue : 0f;

        if (value >= _health.MaxValue)
        {
            Layout();
            _trailPart = part;
            Show(_trail, part);
        }
        else
        {
            _trailFrom = _trailPart;
            _trailTimer = 0f;
        }

        _fillPart = part;
        Show(_fill, part);
    }

    private void Layout()
    {
        Bounds bounds = _body.bounds;
        Vector3 scale = transform.lossyScale;
        float width = bounds.size.x * _widthShare;
        Vector3 leftEdge = new Vector3(bounds.center.x - width * 0.5f, bounds.min.y - _gap, transform.position.z);

        _bar.localPosition = transform.InverseTransformPoint(leftEdge);
        _bar.localScale = new Vector3(width / scale.x, _thickness / scale.y, 1f);
    }

    private void Show(Transform part, float share)
    {
        part.localScale = new Vector3(Mathf.Clamp01(share), 1f, 1f);
    }
}
