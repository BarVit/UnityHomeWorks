using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapNode : MonoBehaviour, IPointerClickHandler
{
    private const float PulseSpeed = 4f;
    private const float PulseAmount = 0.12f;

    [SerializeField] private Image _ring;
    [SerializeField] private GameObject _selection;
    [SerializeField] private Image _core;
    [SerializeField] private GameObject _lock;
    [SerializeField] private TMP_Text _label;
    [SerializeField] private Image[] _stars;
    [SerializeField] private Color _completedColor = new Color(0.21f, 0.85f, 1f);
    [SerializeField] private Color _currentColor = new Color(1f, 0.85f, 0.3f);
    [SerializeField] private Color _openColor = new Color(0.8f, 0.9f, 1f);
    [SerializeField] private Color _lockedColor = new Color(0.45f, 0.5f, 0.58f, 0.85f);
    [SerializeField] private Color _starOn = new Color(1f, 0.82f, 0.3f);
    [SerializeField] private Color _starOff = new Color(0.25f, 0.32f, 0.42f);

    private MapNodeState _state;

    public int Index { get; private set; }

    public MapNodeState State => _state;

    public event Action<MapNode> Clicked;

    private void Update()
    {
        float scale = _state == MapNodeState.Current
            ? 1f + Mathf.Sin(Time.unscaledTime * PulseSpeed) * PulseAmount
            : 1f;

        _ring.transform.localScale = new Vector3(scale, scale, 1f);
    }

    public void Show(int index, string label, MapNodeState state, Color tint)
    {
        Index = index;
        _state = state;
        _label.text = label;
        _lock.SetActive(state == MapNodeState.Locked);

        Color color = GetColor(state);

        _ring.color = color;
        _core.color = state == MapNodeState.Locked ? _lockedColor : tint;
        _label.color = state == MapNodeState.Locked ? _lockedColor : Color.white;
        SetSelected(false);
        SetStars(0);
    }

    public void SetStars(int count)
    {
        for (int i = 0; i < _stars.Length; i++)
        {
            _stars[i].gameObject.SetActive(count > 0);
            _stars[i].color = i < count ? _starOn : _starOff;
        }
    }

    public void SetSelected(bool isSelected)
    {
        _selection.SetActive(isSelected);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Clicked?.Invoke(this);
    }

    private Color GetColor(MapNodeState state)
    {
        switch (state)
        {
            case MapNodeState.Completed:
                return _completedColor;
            case MapNodeState.Current:
                return _currentColor;
            case MapNodeState.Open:
                return _openColor;
            default:
                return _lockedColor;
        }
    }
}
