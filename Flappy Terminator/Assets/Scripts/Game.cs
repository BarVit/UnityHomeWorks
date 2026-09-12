using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] private PlayerShip _playerShip;
    [SerializeField] private EnemySpawner _enemySpawner;
    [SerializeField] private AsteroidSpawner _asteroidSpawner;
    [SerializeField] private WarpJump _warpJump;

    private void OnEnable()
    {
        _enemySpawner.WaveCleared += WinLevel;
    }

    private void Start()
    {
        StartLevel();
    }

    private void OnDisable()
    {
        _enemySpawner.WaveCleared -= WinLevel;
    }

    public void StartLevel()
    {
        _playerShip.EnableControl();
    }

    private void WinLevel()
    {
        _enemySpawner.StopSpawn();
        _asteroidSpawner.StopSpawn();
        _warpJump.Play();
    }
}
