using System.Collections;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] private InputReader _input;
    [SerializeField] private PlayerShip _playerShip;
    [SerializeField] private EnemySpawner _enemySpawner;
    [SerializeField] private AsteroidSpawner _asteroidSpawner;
    [SerializeField] private KillCounter _killCounter;
    [SerializeField] private WarpJump _warpJump;
    [SerializeField] private ScreenNavigator _screens;
    [SerializeField] private PauseController _pause;
    [SerializeField] private LocationRunner _runner;

    private Coroutine _gameOverCountdown;
    private bool _isLevelOver;
    private bool _isRunning;

    private void OnEnable()
    {
        _enemySpawner.WaveCleared += WinLevel;
        _playerShip.Died += LoseLevel;
        _warpJump.Finished += FinishWarp;
        _screens.StartRequested += StartLocation;
        _screens.ResumeRequested += Resume;
        _screens.RestartFromPauseRequested += RestartFromPause;
        _screens.RestartRequested += RestartRun;
        _input.PauseToggled += TogglePause;
        _input.Shot += SkipCutscene;
    }

    private void Start()
    {
        _pause.Resume();
        _playerShip.DisableControl();
        _playerShip.ReturnToStart();
        _screens.ShowStart();
    }

    private void OnDisable()
    {
        _enemySpawner.WaveCleared -= WinLevel;
        _playerShip.Died -= LoseLevel;
        _warpJump.Finished -= FinishWarp;
        _screens.StartRequested -= StartLocation;
        _screens.ResumeRequested -= Resume;
        _screens.RestartFromPauseRequested -= RestartFromPause;
        _screens.RestartRequested -= RestartRun;
        _input.PauseToggled -= TogglePause;
        _input.Shot -= SkipCutscene;
    }

    private void SkipCutscene()
    {
        if (_pause.IsPaused)
            return;

        if (_warpJump.IsPlaying == false)
            return;

        _input.ConsumeFire();
        _warpJump.Skip();
    }

    private void TogglePause()
    {
        if (_screens.TryGoBack())
            return;

        if (_isRunning == false)
            return;

        if (_pause.IsPaused)
            Resume();
        else
            Pause();
    }

    private void Pause()
    {
        _pause.Pause();
        _screens.ShowPause();
    }

    private void Resume()
    {
        _pause.Resume();
        _screens.HidePause();
    }

    private void StartLocation()
    {
        _runner.StartLocation();
        _killCounter.Restart();

        StartLevel();
    }

    private void RestartRun()
    {
        _runner.RestartRun();
        _killCounter.Restart();

        StartLevel();
    }

    private void RestartFromPause()
    {
        Resume();
        RestartRun();
    }

    private void FinishWarp()
    {
        bool wasLastLevel = _runner.IsLastLevel;

        _runner.BankLevel();

        if (wasLastLevel)
            CompleteLocation();
        else
            StartNextLevel();
    }

    private void StartNextLevel()
    {
        _runner.Advance();

        StartLevel();
    }

    private void CompleteLocation()
    {
        _runner.CompleteLocation();
        _isRunning = false;
        _warpJump.Stop();
        _asteroidSpawner.StopSpawn();
        _playerShip.DisableControl();
        _playerShip.ReturnToStart();
        _screens.ShowStart();
    }

    private void StartLevel()
    {
        StopGameOverCountdown();
        _warpJump.Stop();

        _isLevelOver = false;
        _isRunning = true;
        _screens.HideOverlays();
        _playerShip.Restart();
        _enemySpawner.StartWave(_runner.WaveSize, _runner.LocationId);
        _asteroidSpawner.StartSpawn();
    }

    private void LoseLevel()
    {
        if (_isLevelOver)
            return;

        _isLevelOver = true;
        _runner.DropPendingReward();
        _enemySpawner.StopSpawn();
        _asteroidSpawner.StopSpawn();
        _gameOverCountdown = StartCoroutine(ShowGameOver());
    }

    private void WinLevel()
    {
        if (_isLevelOver)
            return;

        _isLevelOver = true;
        _enemySpawner.FinishWave();
        _asteroidSpawner.StopSpawn();
        _warpJump.Play();
    }

    private IEnumerator ShowGameOver()
    {
        yield return new WaitForSeconds(_playerShip.DeathDuration);

        _gameOverCountdown = null;
        _isRunning = false;
        _screens.ShowGameOver(_killCounter.Kills);
    }

    private void StopGameOverCountdown()
    {
        if (_gameOverCountdown == null)
            return;

        StopCoroutine(_gameOverCountdown);
        _gameOverCountdown = null;
    }
}
