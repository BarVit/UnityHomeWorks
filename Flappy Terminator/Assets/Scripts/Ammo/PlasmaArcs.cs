using UnityEngine;

public class PlasmaArcs : MonoBehaviour
{
    [SerializeField] private Material _material;
    [SerializeField] private int _count = 9;
    [SerializeField] private int _segments = 9;
    [SerializeField] private float _innerRadius = 0.64f;
    [SerializeField] private float _outerRadius = 1.92f;
    [SerializeField] private float _jitter = 0.13f;
    [SerializeField] private float _interval = 0.05f;
    [SerializeField] private float _width = 0.035f;
    [SerializeField] private float _tipShare = 0.12f;
    [SerializeField] private float _branchChance = 0.55f;
    [SerializeField] private int _sortingOrder = 3;

    private LineRenderer[] _bolts;
    private LineRenderer[] _branches;
    private Vector3[] _points;
    private Vector3[] _branchPoints;
    private float _timeLeft;

    private int BranchSegments => Mathf.Max(2, _segments / 2);

    private void Awake()
    {
        _points = new Vector3[_segments];
        _branchPoints = new Vector3[BranchSegments];
        _bolts = new LineRenderer[_count];
        _branches = new LineRenderer[_count];

        for (int i = 0; i < _count; i++)
        {
            _bolts[i] = Create("Bolt " + (i + 1), _segments, 1f);
            _branches[i] = Create("Branch " + (i + 1), BranchSegments, 0.55f);
        }
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
        Redraw();
    }

    private void Redraw()
    {
        float width = _width * Mathf.Max(0.0001f, transform.lossyScale.x);

        for (int i = 0; i < _bolts.Length; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            float reach = Random.Range(_outerRadius * 0.55f, _outerRadius);

            Lay(_bolts[i], _points, direction, reach, width);
            Branch(_branches[i], direction, reach, width);
        }
    }

    private void Lay(LineRenderer bolt, Vector3[] points, Vector2 direction, float reach, float width)
    {
        Vector2 side = new Vector2(-direction.y, direction.x);
        int last = points.Length - 1;

        for (int i = 0; i <= last; i++)
        {
            float along = (float)i / last;
            float radius = Mathf.Lerp(_innerRadius, reach, along);
            float wobble = i == 0 || i == last
                ? 0f
                : Random.Range(-_jitter, _jitter) * (1f - Mathf.Abs(along - 0.5f));

            points[i] = direction * radius + side * wobble;
        }

        bolt.widthCurve = AnimationCurve.Linear(0f, width, 1f, width * _tipShare);
        bolt.SetPositions(points);
        bolt.enabled = true;
    }

    private void Branch(LineRenderer branch, Vector2 direction, float reach, float width)
    {
        if (Random.value > _branchChance)
        {
            branch.enabled = false;

            return;
        }

        int from = Random.Range(2, _points.Length - 1);
        Vector2 root = _points[from];
        float turn = Random.Range(25f, 55f) * (Random.value < 0.5f ? -1f : 1f);
        Vector2 away = Quaternion.Euler(0f, 0f, turn) * direction;
        float length = (reach - root.magnitude) * Random.Range(0.5f, 0.95f) + 0.08f;
        Vector2 side = new Vector2(-away.y, away.x);
        int last = _branchPoints.Length - 1;

        for (int i = 0; i <= last; i++)
        {
            float along = (float)i / last;
            float wobble = i == 0 || i == last ? 0f : Random.Range(-_jitter, _jitter) * 0.5f;

            _branchPoints[i] = root + away * (length * along) + side * wobble;
        }

        branch.widthCurve = AnimationCurve.Linear(0f, width * 0.6f, 1f, width * _tipShare);
        branch.SetPositions(_branchPoints);
        branch.enabled = true;
    }

    private LineRenderer Create(string name, int points, float widthScale)
    {
        GameObject holder = new GameObject(name);

        holder.transform.SetParent(transform, false);

        LineRenderer bolt = holder.AddComponent<LineRenderer>();

        bolt.useWorldSpace = false;
        bolt.positionCount = points;
        bolt.numCapVertices = 2;
        bolt.alignment = LineAlignment.View;
        bolt.textureMode = LineTextureMode.Stretch;
        bolt.sharedMaterial = _material;
        bolt.sortingOrder = _sortingOrder;
        bolt.widthMultiplier = widthScale;
        bolt.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        bolt.receiveShadows = false;

        return bolt;
    }
}
