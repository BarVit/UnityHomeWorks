using System.Collections.Generic;
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

    private readonly List<Transform> _targets = new List<Transform>();

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
        _targets.Clear();
    }

    public void Latch(Collider2D[] found, int count)
    {
        _targets.Clear();

        for (int i = 0; i < count; i++)
            _targets.Add(found[i].transform);
    }

    private void Update()
    {
        _timeLeft -= Time.deltaTime;

        if (_timeLeft > 0f)
            return;

        _timeLeft = _interval;

        int aimed = 0;

        for (int i = 0; i < _arcs.Length; i++)
        {
            Vector2 target;

            if (TryGetTarget(aimed, out target))
            {
                Strike(_arcs[i], target);
                aimed++;

                continue;
            }

            Draw(_arcs[i]);
        }
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

    private bool TryGetTarget(int index, out Vector2 local)
    {
        local = Vector2.zero;

        if (index >= _targets.Count)
            return false;

        Transform target = _targets[index];

        if (target == null || target.gameObject.activeInHierarchy == false)
            return false;

        local = transform.InverseTransformPoint(target.position);

        return local.magnitude <= _outerRadius;
    }

    private void Strike(LineRenderer arc, Vector2 target)
    {
        float reach = target.magnitude;
        Vector2 direction = reach <= Mathf.Epsilon ? Vector2.right : target / reach;
        Vector2 side = new Vector2(-direction.y, direction.x);

        Lay(arc, direction, side, Mathf.Max(_innerRadius, reach));
    }

    private void Draw(LineRenderer arc)
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float reach = Random.Range(_outerRadius * 0.6f, _outerRadius);
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        Vector2 side = new Vector2(-direction.y, direction.x);

        Lay(arc, direction, side, reach);
    }

    private void Lay(LineRenderer arc, Vector2 direction, Vector2 side, float reach)
    {
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
