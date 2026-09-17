using UnityEngine;
using UnityEngine.UI;

public class TouchControls : MonoBehaviour
{
    [SerializeField] private InputReader _input;
    [SerializeField] private LevelProgression _levels;
    [SerializeField] private GameObject _panel;
    [SerializeField] private Button _upButton;
    [SerializeField] private Button _downButton;
    [SerializeField] private Button _fireButton;
    [SerializeField] private Button _pauseButton;
    [SerializeField] private bool _showOnDesktop = true;

    private void OnEnable()
    {
        _upButton.onClick.AddListener(_input.Jump);
        _downButton.onClick.AddListener(_input.Descend);
        _fireButton.onClick.AddListener(_input.Shoot);
        _pauseButton.onClick.AddListener(_input.TogglePause);
    }

    private void Start()
    {
        _panel.SetActive(Application.isMobilePlatform || _showOnDesktop);
        _downButton.gameObject.SetActive(HasDownControl());
    }

    private void OnDisable()
    {
        _upButton.onClick.RemoveListener(_input.Jump);
        _downButton.onClick.RemoveListener(_input.Descend);
        _fireButton.onClick.RemoveListener(_input.Shoot);
        _pauseButton.onClick.RemoveListener(_input.TogglePause);
    }

    private bool HasDownControl()
    {
        return _levels.FlightMode == FlightMode.Fixed;
    }
}
