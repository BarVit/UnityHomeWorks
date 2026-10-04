using UnityEngine;

public class PlasmaTether : MonoBehaviour
{
    [SerializeField] private Material _material;
    [SerializeField] private int _count = 4;
    [SerializeField] private int _segments = 11;
    [SerializeField] private float _innerRadius = 0.64f;
    [SerializeField] private float _outerRadius = 1.92f;
    [SerializeField] private float _jitter = 0.11f;
    [SerializeField] private float _width = 0.045f;
    [SerializeField] private float _tipShare = 0.4f;
    [SerializeField] private float _minWaveRate = 6f;
    [SerializeField] private float _maxWaveRate = 16f;
    [SerializeField] private int _sortingOrder = 4;

    private readonly Collider2D[] _found = new Collider2D[8];

    private ContactFilter2D _filter;
    private bool _isAiming;
    private LineRenderer[] _lines;
    private Transform[] _targets;
    private float[][] _phases;
    private float[][] _rates;
    private Vector3[] _points;

    public int HeldCount { get; private set; }

    private void Awake()
    {
        _points = new Vector3[_segments];
        _lines = new LineRenderer[_count];
        _targets = new Transform[_count];
        _phases = new float[_count][];
        _rates = new float[_count][];

        for (int i = 0; i < _count; i++)
        {
            _lines[i] = Create(i);
            _phases[i] = new float[_segments];
            _rates[i] = new float[_segments];
        }
    }

    private void OnEnable()
    {
        for (int i = 0; i < _count; i++)
            Release(i);
    }

    private void Update()
    {
        int found = Find();

        Keep(found);
        Take(found);

        HeldCount = 0;

        for (int i = 0; i < _lines.Length; i++)
        {
            if (_targets[i] == null)
                continue;

            Draw(i);
            HeldCount++;
        }
    }

    public void Aim(ContactFilter2D filter)
    {
        _filter = filter;
        _isAiming = true;
    }

    private int Find()
    {
        if (_isAiming == false)
            return 0;

        float scale = Mathf.Max(0.0001f, transform.lossyScale.x);
        int count = Physics2D.OverlapCircle(transform.position, _outerRadius * scale, _filter, _found);
        int kept = 0;

        for (int i = 0; i < count; i++)
        {
            if (IsInRing(_found[i].transform) == false)
                continue;

            _found[kept] = _found[i];
            kept++;
        }

        return kept;
    }

    private bool IsInRing(Transform target)
    {
        if (target == null || target.gameObject.activeInHierarchy == false)
            return false;

        float distance = ((Vector2)transform.InverseTransformPoint(target.position)).magnitude;

        return distance > _innerRadius && distance <= _outerRadius;
    }

    private void Keep(int found)
    {
        for (int i = 0; i < _targets.Length; i++)
        {
            if (_targets[i] == null)
                continue;

            if (IsHeldAmong(_targets[i], found) == false)
                Release(i);
        }
    }

    private bool IsHeldAmong(Transform target, int found)
    {
        for (int i = 0; i < found; i++)
        {
            if (_found[i].transform == target)
                return true;
        }

        return false;
    }

    private void Take(int found)
    {
        for (int i = 0; i < found; i++)
        {
            if (IsTaken(_found[i].transform))
                continue;

            int free = GetFreeLine();

            if (free < 0)
                return;

            Latch(free, _found[i].transform);
        }
    }

    private bool IsTaken(Transform target)
    {
        foreach (Transform held in _targets)
        {
            if (held == target)
                return true;
        }

        return false;
    }

    private int GetFreeLine()
    {
        for (int i = 0; i < _targets.Length; i++)
        {
            if (_targets[i] == null)
                return i;
        }

        return -1;
    }

    private void Latch(int index, Transform target)
    {
        _targets[index] = target;

        for (int joint = 0; joint < _segments; joint++)
        {
            _phases[index][joint] = Random.Range(0f, Mathf.PI * 2f);
            _rates[index][joint] = Random.Range(_minWaveRate, _maxWaveRate);
        }

        _lines[index].enabled = true;
    }

    private void Release(int index)
    {
        _targets[index] = null;
        _lines[index].enabled = false;
    }

    private void Draw(int index)
    {
        Vector2 local = transform.InverseTransformPoint(_targets[index].position);
        float reach = Mathf.Max(_innerRadius + 0.05f, local.magnitude);
        Vector2 direction = local / local.magnitude;
        Vector2 side = new Vector2(-direction.y, direction.x);
        int last = _segments - 1;

        for (int joint = 0; joint <= last; joint++)
        {
            float along = (float)joint / last;
            float radius = Mathf.Lerp(_innerRadius, reach, along);
            float wave = Mathf.Sin(Time.time * _rates[index][joint] + _phases[index][joint]);

            _points[joint] = direction * radius + side * (wave * _jitter * Mathf.Sin(Mathf.PI * along));
        }

        float width = _width * Mathf.Max(0.0001f, transform.lossyScale.x);

        _lines[index].widthCurve = AnimationCurve.Linear(0f, width, 1f, width * _tipShare);
        _lines[index].SetPositions(_points);
    }

    private LineRenderer Create(int index)
    {
        GameObject holder = new GameObject("Tether " + (index + 1));

        holder.transform.SetParent(transform, false);

        LineRenderer line = holder.AddComponent<LineRenderer>();

        line.useWorldSpace = false;
        line.positionCount = _segments;
        line.numCapVertices = 3;
        line.alignment = LineAlignment.View;
        line.textureMode = LineTextureMode.Stretch;
        line.sharedMaterial = _material;
        line.sortingOrder = _sortingOrder;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.enabled = false;

        return line;
    }
}
