using System;
using System.Collections;
using UnityEngine;

public class Game : MonoBehaviour
{
    private const int FirstLevel = 1;
    private const float NormalTimeScale = 1f;
    private const float PausedTimeScale = 0f;

    [SerializeField] private InputReader _input;
    [SerializeField] private PlayerShip _playerShip;
    [SerializeField] private EnemySpawner _enemySpawner;
    [SerializeField] private AsteroidSpawner _asteroidSpawner;
    [SerializeField] private KillCounter _killCounter;
    [SerializeField] private WarpJump _warpJump;
    [SerializeField] private StartScreen _startScreen;
    [SerializeField] private PauseScreen _pauseScreen;
    [SerializeField] private GameOverScreen _gameOverScreen;
    [SerializeField] private float _gameOverDelay = 2f;
    [SerializeField] private int _firstWaveSize = 6;
    [SerializeField] private int _waveSizeGrowth = 3;

    private Coroutine _gameOverCountdown;
    private int _level = FirstLevel;
    private bool _isLevelOver;
    private bool _isRunning;
    private bool _isPaused;

    public event Action<int> LevelChanged;

    private void OnEnable()
    {
        _enemySpawner.WaveCleared += WinLevel;
        _playerShip.Died += LoseLevel;
        _warpJump.Finished += StartNextLevel;
        _startScreen.StartRequested += StartGame;
        _pauseScreen.ResumeRequested += Resume;
        _pauseScreen.RestartRequested += RestartFromPause;
        _gameOverScreen.RestartRequested += RestartGame;
        _input.PauseToggled += TogglePause;
    }

    private void Start()
    {
        Time.timeScale = NormalTimeScale;

        _playerShip.DisableControl();
        _pauseScreen.Hide();
        _gameOverScreen.Hide();
        _startScreen.Show();
    }

    private void OnDisable()
    {
        _enemySpawner.WaveCleared -= WinLevel;
        _playerShip.Died -= LoseLevel;
        _warpJump.Finished -= StartNextLevel;
        _startScreen.StartRequested -= StartGame;
        _pauseScreen.ResumeRequested -= Resume;
        _pauseScreen.RestartRequested -= RestartFromPause;
        _gameOverScreen.RestartRequested -= RestartGame;
        _input.PauseToggled -= TogglePause;
    }

    private void TogglePause()
    {
        if (_isRunning == false)
            return;

        if (_isPaused)
            Resume();
        else
            Pause();
    }

    private void StartGame()
    {
        _startScreen.Hide();
        RestartGame();
    }

    private void RestartGame()
    {
        _level = FirstLevel;
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
        _level++;

        StartLevel();
    }

    private void StartLevel()
    {
        StopGameOverCountdown();
        _warpJump.Stop();

        _isLevelOver = false;
        _isRunning = true;
        _gameOverScreen.Hide();
        _playerShip.Restart();
        _enemySpawner.StartWave(GetWaveSize());
        _asteroidSpawner.StartSpawn();

        LevelChanged?.Invoke(_level);
    }

    private void Pause()
    {
        _isPaused = true;
        Time.timeScale = PausedTimeScale;
        _playerShip.LockInput();
        _pauseScreen.Show();
    }

    private void Resume()
    {
        _isPaused = false;
        Time.timeScale = NormalTimeScale;
        _playerShip.UnlockInput();
        _pauseScreen.Hide();
    }

    private int GetWaveSize()
    {
        return _firstWaveSize + (_level - FirstLevel) * _waveSizeGrowth;
    }

    private void LoseLevel()
    {
        if (_isLevelOver)
            return;

        _isLevelOver = true;
        _enemySpawner.StopSpawn();
        _asteroidSpawner.StopSpawn();
        _gameOverCountdown = StartCoroutine(ShowGameOver());
    }

    private void WinLevel()
    {
        if (_isLevelOver)
            return;

        _isLevelOver = true;
        _enemySpawner.StopSpawn();
        _asteroidSpawner.StopSpawn();
        _warpJump.Play();
    }

    private IEnumerator ShowGameOver()
    {
        yield return new WaitForSeconds(_gameOverDelay);

        _gameOverCountdown = null;
        _isRunning = false;
        _gameOverScreen.Show(_killCounter.Kills);
    }

    private void StopGameOverCountdown()
    {
        if (_gameOverCountdown == null)
            return;

        StopCoroutine(_gameOverCountdown);
        _gameOverCountdown = null;
    }
}
