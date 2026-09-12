using System;
using UnityEngine;
using UnityEngine.UI;

public class PauseScreen : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _restartButton;

    public event Action ResumeRequested;
    public event Action RestartRequested;

    private void OnEnable()
    {
        _resumeButton.onClick.AddListener(OnResumeClicked);
        _restartButton.onClick.AddListener(OnRestartClicked);
    }

    private void OnDisable()
    {
        _resumeButton.onClick.RemoveListener(OnResumeClicked);
        _restartButton.onClick.RemoveListener(OnRestartClicked);
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
}
