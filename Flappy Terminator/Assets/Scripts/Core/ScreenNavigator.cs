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
    [SerializeField] private MapScreen _mapScreen;

    private bool _isSettingsFromPause;

    public bool IsSettingsOpen { get; private set; }

    public bool CanResumeSession { get; set; }

    public event Action StartRequested;
    public event Action SessionResumeRequested;
    public event Action StartTestRequested;
    public event Action ResumeRequested;
    public event Action RestartFromPauseRequested;
    public event Action RestartRequested;
    public event Action PlayRequested;
    public event Action<string> LocationPlayRequested;

    private void OnEnable()
    {
        _startScreen.StartRequested += OnStartRequested;
        _startScreen.ResumeRequested += OnSessionResumeRequested;
        _startScreen.TestRequested += OnStartTestRequested;
        _startScreen.SettingsRequested += OpenSettingsFromStart;
        _pauseScreen.ResumeRequested += OnResumeRequested;
        _pauseScreen.RestartRequested += OnRestartFromPauseRequested;
        _pauseScreen.SettingsRequested += OpenSettingsFromPause;
        _gameOverScreen.RestartRequested += OnRestartRequested;
        _dockScreen.MenuRequested += ShowStart;
        _dockScreen.PlayRequested += OnPlayRequested;
        _dockScreen.MapRequested += ShowMap;
        _mapScreen.DockRequested += HideMap;
        _mapScreen.MenuRequested += ShowStart;
        _mapScreen.PlayRequested += OnPlayRequested;
        _mapScreen.LocationPlayRequested += OnLocationPlayRequested;
        _settingsScreen.CloseRequested += CloseSettings;
    }

    private void OnDisable()
    {
        _startScreen.StartRequested -= OnStartRequested;
        _startScreen.ResumeRequested -= OnSessionResumeRequested;
        _startScreen.TestRequested -= OnStartTestRequested;
        _startScreen.SettingsRequested -= OpenSettingsFromStart;
        _pauseScreen.ResumeRequested -= OnResumeRequested;
        _pauseScreen.RestartRequested -= OnRestartFromPauseRequested;
        _pauseScreen.SettingsRequested -= OpenSettingsFromPause;
        _gameOverScreen.RestartRequested -= OnRestartRequested;
        _dockScreen.MenuRequested -= ShowStart;
        _dockScreen.PlayRequested -= OnPlayRequested;
        _dockScreen.MapRequested -= ShowMap;
        _mapScreen.DockRequested -= HideMap;
        _mapScreen.MenuRequested -= ShowStart;
        _mapScreen.PlayRequested -= OnPlayRequested;
        _mapScreen.LocationPlayRequested -= OnLocationPlayRequested;
        _settingsScreen.CloseRequested -= CloseSettings;
    }

    public void ShowStart()
    {
        _pauseScreen.Hide();
        _gameOverScreen.Hide();
        _dockScreen.Hide();
        _mapScreen.Hide();
        _settingsScreen.Hide();
        _startScreen.Show(CanResumeSession);
    }

    public void ShowMap()
    {
        _mapScreen.Show(_dockScreen.CanPlay);
    }

    public void HideMap()
    {
        _mapScreen.Hide();
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

    public void ShowDock(Reward earned, bool canPlay, DockReason reason)
    {
        _pauseScreen.Hide();
        _gameOverScreen.Hide();
        _dockScreen.Show(earned, canPlay, reason);
    }

    public void HideOverlays()
    {
        IsSettingsOpen = false;
        _gameOverScreen.Hide();
        _dockScreen.Hide();
        _mapScreen.Hide();
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

    private void OnSessionResumeRequested()
    {
        _startScreen.Hide();
        SessionResumeRequested?.Invoke();
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

    private void OnPlayRequested()
    {
        PlayRequested?.Invoke();
    }

    private void OnLocationPlayRequested(string locationId)
    {
        LocationPlayRequested?.Invoke(locationId);
    }
}
