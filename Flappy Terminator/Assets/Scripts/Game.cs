using System;
using System.Collections;
using UnityEngine;

public class Game : MonoBehaviour
{
    private const int FirstLevel = 1;

    [SerializeField] private PlayerShip _playerShip;
    [SerializeField] private EnemySpawner _enemySpawner;
    [SerializeField] private AsteroidSpawner _asteroidSpawner;
    [SerializeField] private KillCounter _killCounter;
    [SerializeField] private WarpJump _warpJump;
    [SerializeField] private GameOverScreen _gameOverScreen;
    [SerializeField] private float _gameOverDelay = 2f;
    [SerializeField] private int _firstWaveSize = 6;
    [SerializeField] private int _waveSizeGrowth = 3;

    private Coroutine _gameOverCountdown;
    private int _level = FirstLevel;
    private bool _isLevelOver;

    public event Action<int> LevelChanged;

    private void OnEnable()
    {
        _enemySpawner.WaveCleared += WinLevel;
        _playerShip.Died += LoseLevel;
        _warpJump.Finished += StartNextLevel;
        _gameOverScreen.RestartRequested += RestartGame;
    }

    private void Start()
    {
        RestartGame();
    }

    private void OnDisable()
    {
        _enemySpawner.WaveCleared -= WinLevel;
        _playerShip.Died -= LoseLevel;
        _warpJump.Finished -= StartNextLevel;
        _gameOverScreen.RestartRequested -= RestartGame;
    }

    public void RestartGame()
    {
        _level = FirstLevel;
        _killCounter.Restart();

        StartLevel();
    }

    private void StartNextLevel()
    {
        _level++;

        StartLevel();
    }

    private void StartLevel()
    {
        StopGameOverCountdown();

        _isLevelOver = false;
        _gameOverScreen.Hide();
        _playerShip.Restart();
        _enemySpawner.StartWave(GetWaveSize());
        _asteroidSpawner.StartSpawn();

        LevelChanged?.Invoke(_level);
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
