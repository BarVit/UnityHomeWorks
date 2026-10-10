using System;
using UnityEngine;
using UnityEngine.Serialization;

public class ScreenNavigator : MonoBehaviour
{
    [SerializeField] private StartScreen _startScreen;
    [SerializeField] private PauseScreen _pauseScreen;
    [SerializeField] private SettingsScreen _settingsScreen;
    [SerializeField] private GameOverScreen _gameOverScreen;
    [FormerlySerializedAs("_victoryScreen")]
    [SerializeField] private DockScreen _dockScreen;

    private bool _isSettingsFromPause;

    public bool IsSettingsOpen { get; private set; }

    public event Action StartRequested;
    public event Action StartTestRequested;
    public event Action ResumeRequested;
    public event Action RestartFromPauseRequested;
    public event Action RestartRequested;
    public event Action NextLocationRequested;

    private void OnEnable()
    {
        _startScreen.StartRequested += OnStartRequested;
        _startScreen.TestRequested += OnStartTestRequested;
        _startScreen.SettingsRequested += OpenSettingsFromStart;
        _pauseScreen.ResumeRequested += OnResumeRequested;
        _pauseScreen.RestartRequested += OnRestartFromPauseRequested;
        _pauseScreen.SettingsRequested += OpenSettingsFromPause;
        _gameOverScreen.RestartRequested += OnRestartRequested;
        _dockScreen.MenuRequested += ShowStart;
        _dockScreen.NextRequested += OnNextLocationRequested;
        _settingsScreen.CloseRequested += CloseSettings;
    }

    private void OnDisable()
    {
        _startScreen.StartRequested -= OnStartRequested;
        _startScreen.TestRequested -= OnStartTestRequested;
        _startScreen.SettingsRequested -= OpenSettingsFromStart;
        _pauseScreen.ResumeRequested -= OnResumeRequested;
        _pauseScreen.RestartRequested -= OnRestartFromPauseRequested;
        _pauseScreen.SettingsRequested -= OpenSettingsFromPause;
        _gameOverScreen.RestartRequested -= OnRestartRequested;
        _dockScreen.MenuRequested -= ShowStart;
        _dockScreen.NextRequested -= OnNextLocationRequested;
        _settingsScreen.CloseRequested -= CloseSettings;
    }

    public void ShowStart()
    {
        _pauseScreen.Hide();
        _gameOverScreen.Hide();
        _dockScreen.Hide();
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

    public void ShowGameOver(Reward reward)
    {
        _gameOverScreen.Show(reward);
    }

    public void ShowDock(Reward earned, bool hasNextLocation)
    {
        _pauseScreen.Hide();
        _gameOverScreen.Hide();
        _dockScreen.Show(earned, hasNextLocation);
    }

    public void HideOverlays()
    {
        IsSettingsOpen = false;
        _gameOverScreen.Hide();
        _dockScreen.Hide();
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

    private void OnStartTestRequested()
    {
        _startScreen.Hide();
        StartTestRequested?.Invoke();
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

    private void OnNextLocationRequested()
    {
        NextLocationRequested?.Invoke();
    }
}
