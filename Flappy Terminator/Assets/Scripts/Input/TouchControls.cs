using UnityEngine;
using UnityEngine.UI;

public class TouchControls : MonoBehaviour
{
    [SerializeField] private InputReader _input;
    [SerializeField] private LocationRunner _levels;
    [SerializeField] private GameObject _panel;
    [SerializeField] private Button _upButton;
    [SerializeField] private Button _downButton;
    [SerializeField] private HoldButton _fireButton;
    [SerializeField] private Button _pauseButton;
    [SerializeField] private RectTransform[] _fireNeighbours;
    [SerializeField] private bool _showOnDesktop = true;
    [SerializeField] private float _mobileScale = 1.5f;

    private void OnEnable()
    {
        _upButton.onClick.AddListener(_input.Jump);
        _downButton.onClick.AddListener(_input.Descend);
        _fireButton.Pressed += _input.HoldFire;
        _fireButton.Pressed += _input.Shoot;
        _fireButton.Released += _input.ReleaseFire;
        _pauseButton.onClick.AddListener(_input.TogglePause);
    }

    private void Start()
    {
        _panel.SetActive(Application.isMobilePlatform || _showOnDesktop);
        _downButton.gameObject.SetActive(HasDownControl());

        if (Application.isMobilePlatform)
            EnlargeForFingers();
    }

    private void OnDisable()
    {
        _upButton.onClick.RemoveListener(_input.Jump);
        _downButton.onClick.RemoveListener(_input.Descend);
        _fireButton.Pressed -= _input.HoldFire;
        _fireButton.Pressed -= _input.Shoot;
        _fireButton.Released -= _input.ReleaseFire;
        _pauseButton.onClick.RemoveListener(_input.TogglePause);
    }

    private void EnlargeForFingers()
    {
        Enlarge(_upButton.transform);
        Enlarge(_downButton.transform);
        Enlarge(_fireButton.transform);

        foreach (RectTransform neighbour in _fireNeighbours)
            neighbour.anchoredPosition *= _mobileScale;
    }

    private void Enlarge(Transform button)
    {
        RectTransform rect = (RectTransform)button;

        rect.localScale *= _mobileScale;
        rect.anchoredPosition *= _mobileScale;
    }

    private bool HasDownControl()
    {
        return _levels.FlightMode == FlightMode.Fixed;
    }
}
