using UnityEngine;

public class PlasmaTether : MonoBehaviour
{
    [SerializeField] private Material _material;
    [SerializeField] private int _count = 16;
    [SerializeField] private int _segments = 11;
    [SerializeField] private float _innerRadius = 0.64f;
    [SerializeField] private float _jitter = 0.11f;
    [SerializeField] private float _width = 0.045f;
    [SerializeField] private float _tipShare = 0.4f;
    [SerializeField] private float _minWaveRate = 6f;
    [SerializeField] private float _maxWaveRate = 16f;
    [SerializeField] private int _sortingOrder = 4;

    private readonly Transform[] _wanted = new Transform[16];

    private int _wantedCount;
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
        _wantedCount = 0;

        for (int i = 0; i < _count; i++)
            Release(i);
    }

    private void Update()
    {
        Keep();
        Take();

        HeldCount = 0;

        for (int i = 0; i < _lines.Length; i++)
        {
            if (_targets[i] == null)
                continue;

            Draw(i);
            HeldCount++;
        }
    }

    public void Hold(Transform[] targets, int count)
    {
        _wantedCount = Mathf.Min(count, _wanted.Length);

        for (int i = 0; i < _wantedCount; i++)
            _wanted[i] = targets[i];
    }

    private void Keep()
    {
        for (int i = 0; i < _targets.Length; i++)
        {
            if (_targets[i] == null)
                continue;

            if (_targets[i].gameObject.activeInHierarchy == false || IsWanted(_targets[i]) == false)
                Release(i);
        }
    }

    private bool IsWanted(Transform target)
    {
        for (int i = 0; i < _wantedCount; i++)
        {
            if (_wanted[i] == target)
                return true;
        }

        return false;
    }

    private void Take()
    {
        for (int i = 0; i < _wantedCount; i++)
        {
            if (_wanted[i] == null || _wanted[i].gameObject.activeInHierarchy == false || IsTaken(_wanted[i]))
                continue;

            int free = GetFreeLine();

            if (free < 0)
                return;

            Latch(free, _wanted[i]);
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
