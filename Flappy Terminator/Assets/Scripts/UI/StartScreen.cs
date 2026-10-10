using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartScreen : MonoBehaviour
{
    private const string StartText = "START";
    private const string ResumeText = "RESUME";

    [SerializeField] private GameObject _panel;
    [SerializeField] private Button _startButton;
    [SerializeField] private TMP_Text _startLabel;
    [SerializeField] private Button _testButton;
    [SerializeField] private Button _settingsButton;

    private bool _canResume;

    public event Action StartRequested;
    public event Action ResumeRequested;
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

    public void Show(bool canResume)
    {
        _canResume = canResume;
        _startLabel.text = canResume ? ResumeText : StartText;
        Show();
    }

    public void Hide()
    {
        _panel.SetActive(false);
    }

    private void OnStartClicked()
    {
        if (_canResume)
            ResumeRequested?.Invoke();
        else
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
