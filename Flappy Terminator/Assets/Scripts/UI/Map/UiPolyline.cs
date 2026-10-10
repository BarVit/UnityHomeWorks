using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiPolyline : MaskableGraphic
{
    [SerializeField] private List<Vector2> _points = new List<Vector2>();
    [SerializeField] private float _width = 3f;
    [SerializeField] private float _dash;
    [SerializeField] private float _gap;
    [SerializeField] private float _speed;

    private float _offset;

    protected override void Awake()
    {
        base.Awake();
        raycastTarget = false;
    }

    private void Update()
    {
        if (Mathf.Approximately(_speed, 0f) || _dash <= 0f)
            return;

        _offset = Mathf.Repeat(_offset + _speed * Time.unscaledDeltaTime, _dash + _gap);
        SetVerticesDirty();
    }

    public void SetPoints(IList<Vector2> points)
    {
        _points.Clear();
        _points.AddRange(points);
        SetVerticesDirty();
    }

    public void SetStyle(float width, float dash, float gap, float speed)
    {
        _width = width;
        _dash = dash;
        _gap = gap;
        _speed = speed;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vertices)
    {
        vertices.Clear();

        if (_points.Count < 2)
            return;

        Rect rect = rectTransform.rect;
        Vector2 previous = ToLocal(_points[0], rect);
        float travelled = -_offset;

        for (int i = 1; i < _points.Count; i++)
        {
            Vector2 next = ToLocal(_points[i], rect);

            if (_dash <= 0f)
                AddQuad(vertices, previous, next);
            else
                travelled = AddDashes(vertices, previous, next, travelled);

            previous = next;
        }
    }

    private float AddDashes(VertexHelper vertices, Vector2 from, Vector2 to, float travelled)
    {
        float length = Vector2.Distance(from, to);
        float period = _dash + _gap;
        float start = -Mathf.Repeat(travelled, period);

        for (float at = start; at < length; at += period)
        {
            float a = Mathf.Max(at, 0f);
            float b = Mathf.Min(at + _dash, length);

            if (b > a)
                AddQuad(vertices, Vector2.Lerp(from, to, a / length), Vector2.Lerp(from, to, b / length));
        }

        return travelled + length;
    }

    private void AddQuad(VertexHelper vertices, Vector2 from, Vector2 to)
    {
        Vector2 direction = (to - from).normalized;
        Vector2 side = new Vector2(-direction.y, direction.x) * (_width * 0.5f);
        int start = vertices.currentVertCount;
        UIVertex vertex = UIVertex.simpleVert;

        vertex.color = color;

        foreach (Vector2 corner in new[] { from - side, from + side, to + side, to - side })
        {
            vertex.position = corner;
            vertices.AddVert(vertex);
        }

        vertices.AddTriangle(start, start + 1, start + 2);
        vertices.AddTriangle(start, start + 2, start + 3);
    }

    private static Vector2 ToLocal(Vector2 normalized, Rect rect)
    {
        return new Vector2(rect.xMin + normalized.x * rect.width, rect.yMax - normalized.y * rect.height);
    }
}
