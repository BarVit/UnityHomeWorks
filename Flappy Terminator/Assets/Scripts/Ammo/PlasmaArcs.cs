using UnityEngine;

public class PlasmaArcs : MonoBehaviour
{
    [SerializeField] private Material _material;
    [SerializeField] private int _count = 9;
    [SerializeField] private int _segments = 9;
    [SerializeField] private float _innerRadius = 0.64f;
    [SerializeField] private float _outerRadius = 1.28f;
    [SerializeField] private float _jitter = 0.13f;
    [SerializeField] private float _interval = 0.05f;
    [SerializeField] private float _width = 0.035f;
    [SerializeField] private float _tipShare = 0.12f;
    [SerializeField] private float _branchChance = 0.55f;
    [SerializeField] private int _sortingOrder = 3;

    private readonly Collider2D[] _found = new Collider2D[8];

    private ContactFilter2D _filter;
    private bool _isAiming;
    private LineRenderer[] _bolts;
    private LineRenderer[] _branches;
    private Vector3[] _points;
    private Vector3[] _branchPoints;
    private float _timeLeft;

    public int TargetCount { get; private set; }

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
        TargetCount = FindTargets();
        Redraw(TargetCount);
    }

    public void Aim(ContactFilter2D filter)
    {
        _filter = filter;
        _isAiming = true;
    }

    private int FindTargets()
    {
        if (_isAiming == false)
            return 0;

        float scale = Mathf.Max(0.0001f, transform.lossyScale.x);
        int count = Physics2D.OverlapCircle(transform.position, _outerRadius * scale, _filter, _found);
        int kept = 0;

        for (int i = 0; i < count; i++)
        {
            float distance = ((Vector2)transform.InverseTransformPoint(_found[i].transform.position)).magnitude;

            if (distance <= _innerRadius || distance > _outerRadius)
                continue;

            _found[kept] = _found[i];
            kept++;
        }

        return Mathf.Min(kept, _bolts.Length);
    }

    private void Redraw(int targets)
    {
        float width = _width * Mathf.Max(0.0001f, transform.lossyScale.x);

        for (int i = 0; i < _bolts.Length; i++)
        {
            Vector2 direction;
            float reach;

            if (i < targets)
            {
                Vector2 local = transform.InverseTransformPoint(_found[i].transform.position);

                reach = Mathf.Max(_innerRadius + 0.05f, local.magnitude);
                direction = local / reach;
            }
            else
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);

                direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                reach = Random.Range(_outerRadius * 0.55f, _outerRadius);
            }

            Lay(_bolts[i], _points, direction, reach, width);
            Branch(_branches[i], direction, reach, width, i < targets);
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

    private void Branch(LineRenderer branch, Vector2 direction, float reach, float width, bool isAimed)
    {
        if (isAimed == false && Random.value > _branchChance)
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
