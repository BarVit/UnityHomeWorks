using UnityEngine;

[RequireComponent(typeof(PlayerShooter), typeof(Health))]
[RequireComponent(typeof(PlayerMover))]
public class PlayerShip : MonoBehaviour, IDamageable, IRemoveable
{
    private const float ExplosionLifetime = 10f;
    private const float DestroyDelay = 1f;
    private const int PoolCapacity = 1;
    private const int PoolMaxSize = 1;

    [SerializeField] private AmmoHitHandler _ammoHitHandler;
    [SerializeField] private PlayerCollisionHandler _playerCollisionHandler;
    [SerializeField] private Collider2D _bodyCollider;
    [SerializeField] private ExplosionAnimation _explosionAnimation;
    [SerializeField] private AudioSource _audioBumpAsteroid;
    [SerializeField] private AudioSource _audioBumpEnemy;

    private SpawnPool<ExplosionAnimation> _explosionPool;
    private PlayerShooter _playerShooter;
    private Health _health;
    private PlayerMover _playerMover;
    private bool _isControlEnabled = true;
    private int _enemyAmmoLayout = 9;

    private void Awake()
    {
        _playerShooter = GetComponent<PlayerShooter>();
        _health = GetComponent<Health>();
        _playerMover = GetComponent<PlayerMover>();
        _explosionPool = new SpawnPool<ExplosionAnimation>(_explosionAnimation, PoolCapacity, PoolMaxSize);
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
        if (_isControlEnabled == false)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            _playerShooter.Shoot();
        }
    }

    public void EnableControl()
    {
        _isControlEnabled = true;
        _playerMover.enabled = true;
        _playerMover.Resume();
        _bodyCollider.enabled = true;
    }

    public void DisableControl()
    {
        _isControlEnabled = false;
        _playerMover.Stop();
        _playerMover.enabled = false;
        _bodyCollider.enabled = false;
    }

    public void ReturnToStart()
    {
        _playerMover.ReturnToStart();
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
        ExplosionAnimation explosion = _explosionPool.Get(transform.position);

        explosion.Play(ExplosionLifetime);

        Destroy(gameObject, DestroyDelay);
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