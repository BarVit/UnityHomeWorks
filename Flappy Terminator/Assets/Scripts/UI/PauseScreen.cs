using System;
using UnityEngine;
using UnityEngine.UI;

public class PauseScreen : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _settingsButton;

    public event Action ResumeRequested;
    public event Action RestartRequested;
    public event Action SettingsRequested;

    private void OnEnable()
    {
        _resumeButton.onClick.AddListener(OnResumeClicked);
        _restartButton.onClick.AddListener(OnRestartClicked);
        _settingsButton.onClick.AddListener(OnSettingsClicked);
    }

    private void OnDisable()
    {
        _resumeButton.onClick.RemoveListener(OnResumeClicked);
        _restartButton.onClick.RemoveListener(OnRestartClicked);
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

    private void OnResumeClicked()
    {
        ResumeRequested?.Invoke();
    }

    private void OnRestartClicked()
    {
        RestartRequested?.Invoke();
    }

    private void OnSettingsClicked()
    {
        SettingsRequested?.Invoke();
    }
}
