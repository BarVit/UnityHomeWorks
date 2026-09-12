using UnityEngine;

public class PlayerShipAudio : MonoBehaviour
{
    [SerializeField] private PlayerShip _playerShip;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Sound _asteroidBump;
    [SerializeField] private Sound _enemyBump;

    private void OnEnable()
    {
        _playerShip.AsteroidBumped += PlayAsteroidBump;
        _playerShip.EnemyBumped += PlayEnemyBump;
    }

    private void OnDisable()
    {
        _playerShip.AsteroidBumped -= PlayAsteroidBump;
        _playerShip.EnemyBumped -= PlayEnemyBump;
    }

    private void PlayAsteroidBump()
    {
        Play(_asteroidBump);
    }

    private void PlayEnemyBump()
    {
        Play(_enemyBump);
    }

    private void Play(Sound sound)
    {
        _audioSource.PlayOneShot(sound.Clip, sound.Volume);
    }
}
