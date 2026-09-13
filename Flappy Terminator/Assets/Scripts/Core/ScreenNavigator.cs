using System;
using UnityEngine;

public class ScreenNavigator : MonoBehaviour
{
    [SerializeField] private StartScreen _startScreen;
    [SerializeField] private PauseScreen _pauseScreen;
    [SerializeField] private SettingsScreen _settingsScreen;
    [SerializeField] private GameOverScreen _gameOverScreen;

    private bool _isSettingsFromPause;

    public bool IsSettingsOpen { get; private set; }

    public event Action StartRequested;
    public event Action ResumeRequested;
    public event Action RestartFromPauseRequested;
    public event Action RestartRequested;

    private void OnEnable()
    {
        _startScreen.StartRequested += OnStartRequested;
        _startScreen.SettingsRequested += OpenSettingsFromStart;
        _pauseScreen.ResumeRequested += OnResumeRequested;
        _pauseScreen.RestartRequested += OnRestartFromPauseRequested;
        _pauseScreen.SettingsRequested += OpenSettingsFromPause;
        _gameOverScreen.RestartRequested += OnRestartRequested;
        _settingsScreen.CloseRequested += CloseSettings;
    }

    private void OnDisable()
    {
        _startScreen.StartRequested -= OnStartRequested;
        _startScreen.SettingsRequested -= OpenSettingsFromStart;
        _pauseScreen.ResumeRequested -= OnResumeRequested;
        _pauseScreen.RestartRequested -= OnRestartFromPauseRequested;
        _pauseScreen.SettingsRequested -= OpenSettingsFromPause;
        _gameOverScreen.RestartRequested -= OnRestartRequested;
        _settingsScreen.CloseRequested -= CloseSettings;
    }

    public void ShowStart()
    {
        _pauseScreen.Hide();
        _gameOverScreen.Hide();
        _settingsScreen.Hide();
        _startScreen.Show();
    }

    public void ShowPause()
    {
        _pauseScreen.Show();
    }

    public void HidePause()
    {
        _pauseScreen.Hide();
    }

    public void ShowGameOver(int kills)
    {
        _gameOverScreen.Show(kills);
    }

    public void HideOverlays()
    {
        IsSettingsOpen = false;
        _gameOverScreen.Hide();
        _settingsScreen.Hide();
    }

    public bool TryGoBack()
    {
        if (IsSettingsOpen == false)
            return false;

        CloseSettings();

        return true;
    }

    private void OpenSettingsFromStart()
    {
        _isSettingsFromPause = false;
        IsSettingsOpen = true;
        _startScreen.Hide();
        _settingsScreen.Show();
    }

    private void OpenSettingsFromPause()
    {
        _isSettingsFromPause = true;
        IsSettingsOpen = true;
        _pauseScreen.Hide();
        _settingsScreen.Show();
    }

    private void CloseSettings()
    {
        IsSettingsOpen = false;
        _settingsScreen.Hide();

        if (_isSettingsFromPause)
            _pauseScreen.Show();
        else
            _startScreen.Show();
    }

    private void OnStartRequested()
    {
        _startScreen.Hide();
        StartRequested?.Invoke();
    }

    private void OnResumeRequested()
    {
        ResumeRequested?.Invoke();
    }

    private void OnRestartFromPauseRequested()
    {
        RestartFromPauseRequested?.Invoke();
    }

    private void OnRestartRequested()
    {
        RestartRequested?.Invoke();
    }
}
