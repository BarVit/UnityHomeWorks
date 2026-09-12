using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] private EnemySpawner _enemySpawner;
    [SerializeField] private AsteroidSpawner _asteroidSpawner;
    [SerializeField] private WarpJump _warpJump;

    private void OnEnable()
    {
        _enemySpawner.WaveCleared += WinLevel;
    }

    private void OnDisable()
    {
        _enemySpawner.WaveCleared -= WinLevel;
    }

    private void WinLevel()
    {
        _enemySpawner.StopSpawn();
        _asteroidSpawner.StopSpawn();
        _warpJump.Play();
    }
}
