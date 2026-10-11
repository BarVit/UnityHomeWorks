using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapRegion : MaskableGraphic, ICanvasRaycastFilter, IPointerEnterHandler, IPointerExitHandler,
    IPointerClickHandler
{
    [SerializeField] private string _id;
    [SerializeField] private bool _isAvailable;
    [SerializeField] private List<Vector2> _polygon = new List<Vector2>();
    [SerializeField] private Color _hoverColor = new Color(0.4f, 0.85f, 1f, 0.18f);
    [SerializeField] private Color _lockedHoverColor = new Color(0.6f, 0.75f, 1f, 0.025f);
    [SerializeField] private TMP_Text _label;
    [SerializeField] private Color _labelColor = new Color(0.55f, 0.6f, 0.68f);
    [SerializeField] private Color _labelHoverColor = Color.white;

    public string Id => _id;

    public bool IsAvailable => _isAvailable;

    public event Action<MapRegion> Clicked;
    public event Action<MapRegion, bool> Hovered;

    protected override void Awake()
    {
        base.Awake();
        color = Color.clear;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        color = Color.clear;
        LightLabel(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        color = _isAvailable ? _hoverColor : _lockedHoverColor;
        LightLabel(true);
        Hovered?.Invoke(this, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        color = Color.clear;
        LightLabel(false);
        Hovered?.Invoke(this, false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Clicked?.Invoke(this);
    }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera,
                out Vector2 local) == false)
            return false;

        Rect rect = rectTransform.rect;
        Vector2 point = new Vector2((local.x - rect.xMin) / rect.width, (rect.yMax - local.y) / rect.height);

        return Contains(point);
    }

    protected override void OnPopulateMesh(VertexHelper vertices)
    {
        vertices.Clear();

        if (_polygon.Count < 3)
            return;

        Rect rect = rectTransform.rect;
        UIVertex vertex = UIVertex.simpleVert;

        vertex.color = color;

        foreach (Vector2 point in _polygon)
        {
            vertex.position = new Vector2(rect.xMin + point.x * rect.width, rect.yMax - point.y * rect.height);
            vertices.AddVert(vertex);
        }

        foreach (Vector3Int triangle in Triangulate())
            vertices.AddTriangle(triangle.x, triangle.y, triangle.z);
    }

    private void LightLabel(bool isLit)
    {
        if (_label == null || _isAvailable)
            return;

        _label.color = isLit ? _labelHoverColor : _labelColor;
    }

    private bool Contains(Vector2 point)
    {
        bool inside = false;

        for (int i = 0, j = _polygon.Count - 1; i < _polygon.Count; j = i++)
        {
            Vector2 a = _polygon[i];
            Vector2 b = _polygon[j];

            if ((a.y > point.y) != (b.y > point.y) && point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x)
                inside = !inside;
        }

        return inside;
    }

    private List<Vector3Int> Triangulate()
    {
        List<Vector3Int> triangles = new List<Vector3Int>();
        List<int> left = new List<int>();

        for (int i = 0; i < _polygon.Count; i++)
            left.Add(i);

        float winding = SignedArea() > 0f ? 1f : -1f;
        int guard = left.Count * left.Count;

        while (left.Count > 3 && guard-- > 0)
        {
            for (int i = 0; i < left.Count; i++)
            {
                int a = left[(i + left.Count - 1) % left.Count];
                int b = left[i];
                int c = left[(i + 1) % left.Count];

                if (IsEar(a, b, c, left, winding) == false)
                    continue;

                triangles.Add(new Vector3Int(a, b, c));
                left.RemoveAt(i);

                break;
            }
        }

        if (left.Count == 3)
            triangles.Add(new Vector3Int(left[0], left[1], left[2]));

        return triangles;
    }

    private bool IsEar(int a, int b, int c, List<int> left, float winding)
    {
        Vector2 pa = _polygon[a];
        Vector2 pb = _polygon[b];
        Vector2 pc = _polygon[c];

        if (Cross(pb - pa, pc - pb) * winding <= 0f)
            return false;

        foreach (int other in left)
        {
            if (other != a && other != b && other != c && InTriangle(_polygon[other], pa, pb, pc))
                return false;
        }

        return true;
    }

    private float SignedArea()
    {
        float area = 0f;

        for (int i = 0, j = _polygon.Count - 1; i < _polygon.Count; j = i++)
            area += Cross(_polygon[j], _polygon[i]);

        return area;
    }

    private static bool InTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = Cross(b - a, p - a);
        float d2 = Cross(c - b, p - b);
        float d3 = Cross(a - c, p - c);
        bool negative = d1 < 0f || d2 < 0f || d3 < 0f;
        bool positive = d1 > 0f || d2 > 0f || d3 > 0f;

        return (negative && positive) == false;
    }

    private static float Cross(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }
}
