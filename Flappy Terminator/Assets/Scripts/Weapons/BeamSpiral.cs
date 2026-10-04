using UnityEngine;

public class BeamSpiral : MonoBehaviour
{
    private const int PointsPerTurn = 16;
    private const int MaxPoints = 1024;
    private const float NearWidthShare = 1f;
    private const float FarWidthShare = 0.35f;
    private const float EdgeShare = 0.25f;

    [SerializeField] private float _radius = 0.1f;
    [SerializeField] private float _pitch = 0.6f;
    [SerializeField] private float _phase;
    [SerializeField] private bool _isDashed;
    [SerializeField] private Vector2 _dashLength = new Vector2(0.25f, 0.9f);
    [SerializeField] private Vector2 _gapLength = new Vector2(0.1f, 0.45f);
    [SerializeField] private Vector2 _dashAlpha = new Vector2(0.3f, 1f);

    private LineRenderer[] _lines;
    private GradientColorKey[] _colors;
    private float[] _alphas;
    private Gradient[] _gradients;
    private GradientAlphaKey[] _alphaKeys;

    private void Awake()
    {
        _lines = GetComponentsInChildren<LineRenderer>(true);
        _colors = _lines[0].colorGradient.colorKeys;
        _alphas = new float[_lines.Length];
        _gradients = new Gradient[_lines.Length];
        _alphaKeys = new GradientAlphaKey[4];

        for (int i = 0; i < _lines.Length; i++)
            _gradients[i] = new Gradient();
    }

    public void Show(Vector3 from, Vector3 to)
    {
        Vector3 axis = to - from;
        float length = axis.magnitude;

        foreach (LineRenderer line in _lines)
            line.positionCount = 0;

        if (length < 0.001f || _pitch <= 0f)
            return;

        if (_isDashed == false)
        {
            Lay(0, from, axis / length, 0f, length);
            _alphas[0] = 1f;

            return;
        }

        float start = Random.Range(0f, _gapLength.y);

        for (int i = 0; i < _lines.Length && start < length; i++)
        {
            float end = Mathf.Min(length, start + Random.Range(_dashLength.x, _dashLength.y));

            Lay(i, from, axis / length, start, end);
            _alphas[i] = Random.Range(_dashAlpha.x, _dashAlpha.y);
            start = end + Random.Range(_gapLength.x, _gapLength.y);
        }
    }

    public void Fade(float share)
    {
        for (int i = 0; i < _lines.Length; i++)
        {
            if (_lines[i].positionCount == 0)
                continue;

            float alpha = _alphas[i] * share;
            float edge = _isDashed ? 0f : alpha;

            _alphaKeys[0] = new GradientAlphaKey(edge, 0f);
            _alphaKeys[1] = new GradientAlphaKey(alpha, EdgeShare);
            _alphaKeys[2] = new GradientAlphaKey(alpha, 1f - EdgeShare);
            _alphaKeys[3] = new GradientAlphaKey(edge, 1f);
            _gradients[i].SetKeys(_colors, _alphaKeys);
            _lines[i].colorGradient = _gradients[i];
        }
    }

    private void Lay(int index, Vector3 from, Vector3 direction, float start, float end)
    {
        Vector3 side = new Vector3(-direction.y, direction.x, 0f);
        float span = end - start;
        int count = Mathf.Clamp(Mathf.CeilToInt(span / _pitch * PointsPerTurn) + 1, 2, MaxPoints);
        Vector3[] points = new Vector3[count];
        Keyframe[] widths = new Keyframe[count];

        for (int i = 0; i < count; i++)
        {
            float along = (float)i / (count - 1);
            float distance = start + span * along;
            float angle = Mathf.PI * 2f * distance / _pitch + _phase;
            float depth = (Mathf.Cos(angle) + 1f) * 0.5f;

            points[i] = from + direction * distance + side * (Mathf.Sin(angle) * _radius);
            widths[i] = new Keyframe(along, Mathf.Lerp(FarWidthShare, NearWidthShare, depth));
        }

        LineRenderer line = _lines[index];

        line.useWorldSpace = true;
        line.positionCount = count;
        line.SetPositions(points);
        line.widthCurve = new AnimationCurve(widths);
    }
}
