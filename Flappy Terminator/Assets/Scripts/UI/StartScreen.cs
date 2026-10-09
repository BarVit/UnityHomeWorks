using System;
using UnityEngine;
using UnityEngine.UI;

public class StartScreen : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _testButton;
    [SerializeField] private Button _settingsButton;

    public event Action StartRequested;
    public event Action TestRequested;
    public event Action SettingsRequested;

    private void OnEnable()
    {
        _startButton.onClick.AddListener(OnStartClicked);
        _testButton.onClick.AddListener(OnTestClicked);
        _settingsButton.onClick.AddListener(OnSettingsClicked);
    }

    private void OnDisable()
    {
        _startButton.onClick.RemoveListener(OnStartClicked);
        _testButton.onClick.RemoveListener(OnTestClicked);
        _settingsButton.onClick.RemoveListener(OnSettingsClicked);
    }

    public void Show()
    {
        _panel.SetActive(true);
    }

    public void Hide()
    {
        _panel.SetActive(false);
    }

    private void OnStartClicked()
    {
        StartRequested?.Invoke();
    }

    private void OnTestClicked()
    {
        TestRequested?.Invoke();
    }

    private void OnSettingsClicked()
    {
        SettingsRequested?.Invoke();
    }
}
