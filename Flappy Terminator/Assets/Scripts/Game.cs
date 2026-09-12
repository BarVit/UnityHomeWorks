using System.Collections;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] private PlayerShip _playerShip;
    [SerializeField] private EnemySpawner _enemySpawner;
    [SerializeField] private AsteroidSpawner _asteroidSpawner;
    [SerializeField] private KillCounter _killCounter;
    [SerializeField] private WarpJump _warpJump;
    [SerializeField] private GameOverScreen _gameOverScreen;
    [SerializeField] private float _gameOverDelay = 2f;

    private Coroutine _gameOverCountdown;
    private bool _isLevelOver;

    private void OnEnable()
    {
        _enemySpawner.WaveCleared += WinLevel;
        _playerShip.Died += LoseLevel;
        _gameOverScreen.RestartRequested += RestartLevel;
    }

    private void Start()
    {
        StartLevel();
    }

    private void OnDisable()
    {
        _enemySpawner.WaveCleared -= WinLevel;
        _playerShip.Died -= LoseLevel;
        _gameOverScreen.RestartRequested -= RestartLevel;
    }

    public void StartLevel()
    {
        StopGameOverCountdown();
        _isLevelOver = false;
        _gameOverScreen.Hide();
        _playerShip.EnableControl();
    }

    public void RestartLevel()
    {
        StopGameOverCountdown();
        _isLevelOver = false;
        _gameOverScreen.Hide();
        _killCounter.Restart();
        _enemySpawner.Restart();
        _asteroidSpawner.Restart();
        _playerShip.Restart();
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

    private void WinLevel()
    {
        if (_isLevelOver)
            return;

        _isLevelOver = true;
        _enemySpawner.StopSpawn();
        _asteroidSpawner.StopSpawn();
        _warpJump.Play();
    }
}
