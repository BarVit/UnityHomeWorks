using UnityEngine;

public class PlasmaArcs : MonoBehaviour
{
    [SerializeField] private Material _material;
    [SerializeField] private int _count = 9;
    [SerializeField] private int _segments = 9;
    [SerializeField] private float _innerRadius = 0.64f;
    [SerializeField] private float _outerRadius = 1.28f;
    [SerializeField] private float _jitter = 0.13f;
    [SerializeField] private float _interval = 0.06f;
    [SerializeField] private float _width = 0.03f;
    [SerializeField] private int _sortingOrder = 3;

    private LineRenderer[] _arcs;
    private Vector3[] _points;
    private float _timeLeft;

    private void Awake()
    {
        _points = new Vector3[_segments];
        _arcs = new LineRenderer[_count];

        for (int i = 0; i < _count; i++)
            _arcs[i] = Create(i);
    }

    private void OnEnable()
    {
        _timeLeft = 0f;
    }

    private void Update()
    {
        _timeLeft -= Time.deltaTime;

        if (_timeLeft > 0f)
            return;

        _timeLeft = _interval;

        for (int i = 0; i < _arcs.Length; i++)
            Draw(_arcs[i]);
    }

    private LineRenderer Create(int index)
    {
        GameObject holder = new GameObject("Arc " + (index + 1));

        holder.transform.SetParent(transform, false);

        LineRenderer arc = holder.AddComponent<LineRenderer>();

        arc.useWorldSpace = false;
        arc.positionCount = _segments;
        arc.numCapVertices = 2;
        arc.alignment = LineAlignment.View;
        arc.textureMode = LineTextureMode.Stretch;
        arc.sharedMaterial = _material;
        arc.sortingOrder = _sortingOrder;
        arc.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        arc.receiveShadows = false;

        return arc;
    }

    private void Draw(LineRenderer arc)
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float reach = Random.Range(_outerRadius * 0.6f, _outerRadius);
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        Vector2 side = new Vector2(-direction.y, direction.x);

        for (int i = 0; i < _segments; i++)
        {
            float along = (float)i / (_segments - 1);
            float radius = Mathf.Lerp(_innerRadius, reach, along);
            float wobble = i == 0 || i == _segments - 1
                ? 0f
                : Random.Range(-_jitter, _jitter) * (1f - Mathf.Abs(along - 0.5f));

            _points[i] = direction * radius + side * wobble;
        }

        arc.widthMultiplier = _width * transform.lossyScale.x;
        arc.SetPositions(_points);
    }
}
