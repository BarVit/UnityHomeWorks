using UnityEngine;

[RequireComponent(typeof(PlayerShooter), typeof(Health))]
[RequireComponent(typeof(PlayerMover))]
public class PlayerShip : Ship, IDamageable, IRemoveable
{
    [SerializeField] private AmmoHitHandler _ammoHitHandler;
    [SerializeField] private PlayerCollisionHandler _playerCollisionHandler;
    [SerializeField] private ExplosionAnimation _explosionAnimation;
    [SerializeField] private AudioSource _audioBumpAsteroid;
    [SerializeField] private AudioSource _audioBumpEnemy;

    private PlayerShooter _playerShooter;
    private Health _health;
    private PlayerMover _playerMover;
    private Vector2 _startPosition;
    private Quaternion _startRotation;
    private int _enemyAmmoLayout = 9;

    private void Awake()
    {
        _playerShooter = GetComponent<PlayerShooter>();
        _health = GetComponent<Health>();
        _playerMover = GetComponent<PlayerMover>();
        _startPosition = transform.position;
        _startRotation = transform.rotation;
    }

    private void OnEnable()
    {
        _ammoHitHandler.HitDetected += TakeDamage;
        _playerCollisionHandler.AsteroidBumped += TakeDamage;
        _playerCollisionHandler.EnemyBumped += TakeDamage;
        _health.Died += Die;
    }

    private void OnDisable()
    {
        _ammoHitHandler.HitDetected -= TakeDamage;
        _playerCollisionHandler.AsteroidBumped -= TakeDamage;
        _playerCollisionHandler.EnemyBumped -= TakeDamage;
        _health.Died -= Die;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _playerShooter.Shoot();
        }
    }

    public void Reset()
    {
        transform.position = _startPosition;
        transform.rotation = _startRotation;
        _playerMover.Reset();
    }

    public void TakeDamage(Ammo ammo)
    {
        if(ammo.gameObject.layer == _enemyAmmoLayout)
            _health.TakeDamage(ammo.Damage);
    }

    public void TakeDamage(Asteroid asteroid)
    {
        _health.TakeDamage(asteroid.Damage);
        PushOffAsteroid();
        _audioBumpAsteroid.Play();
    }

    public void TakeDamage(EnemyShip enemyShip)
    {
        _health.TakeDamage(enemyShip.BodyDamage);
        PushOffAsteroid();
        _audioBumpEnemy.Play();
    }

    public void Die()
    {
        var exp = Instantiate(_explosionAnimation, transform.position, Quaternion.identity);
        exp.End(10f);
        Destroy(gameObject, 1f);
    }

    public void Remove()
    {
        Die();
    }

    private void PushOffAsteroid()
    {
        _playerMover.Push();
    }
}