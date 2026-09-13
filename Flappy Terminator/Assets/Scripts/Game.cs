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
    [SerializeField] private LevelProgression _levels;

    private Coroutine _gameOverCountdown;
    private bool _isLevelOver;
    private bool _isRunning;

    private void OnEnable()
    {
        _enemySpawner.WaveCleared += WinLevel;
        _playerShip.Died += LoseLevel;
        _warpJump.Finished += StartNextLevel;
        _screens.StartRequested += RestartGame;
        _screens.ResumeRequested += Resume;
        _screens.RestartFromPauseRequested += RestartFromPause;
        _screens.RestartRequested += RestartGame;
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
        _warpJump.Finished -= StartNextLevel;
        _screens.StartRequested -= RestartGame;
        _screens.ResumeRequested -= Resume;
        _screens.RestartFromPauseRequested -= RestartFromPause;
        _screens.RestartRequested -= RestartGame;
        _input.PauseToggled -= TogglePause;
        _input.Shot -= SkipCutscene;
    }

    private void SkipCutscene()
    {
        if (_pause.IsPaused)
            return;

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

    private void RestartGame()
    {
        _levels.Restart();
        _killCounter.Restart();

        StartLevel();
    }

    private void RestartFromPause()
    {
        Resume();
        RestartGame();
    }

    private void StartNextLevel()
    {
        _levels.Advance();

        StartLevel();
    }

    private void StartLevel()
    {
        StopGameOverCountdown();
        _warpJump.Stop();

        _isLevelOver = false;
        _isRunning = true;
        _screens.HideOverlays();
        _playerShip.Restart();
        _enemySpawner.StartWave(_levels.WaveSize);
        _asteroidSpawner.StartSpawn();
    }

    private void LoseLevel()
    {
        if (_isLevelOver)
            return;

        _isLevelOver = true;
        _enemySpawner.StopLevel();
        _asteroidSpawner.StopSpawn();
        _gameOverCountdown = StartCoroutine(ShowGameOver());
    }

    private void WinLevel()
    {
        if (_isLevelOver)
            return;

        _isLevelOver = true;
        _enemySpawner.StopLevel();
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
