using System;
using UnityEngine;

[RequireComponent(typeof(PlayerShooter), typeof(Health))]
[RequireComponent(typeof(PlayerMover))]
public class PlayerShip : MonoBehaviour, IDamageable, IRemoveable
{
    private const int PoolCapacity = 1;
    private const int PoolMaxSize = 1;

    [SerializeField] private InputReader _input;
    [SerializeField] private AmmoHitHandler _ammoHitHandler;
    [SerializeField] private PlayerCollisionHandler _playerCollisionHandler;
    [SerializeField] private Collider2D _bodyCollider;
    [SerializeField] private SpriteRenderer _shipSprite;
    [SerializeField] private ExplosionAnimation _explosionAnimation;
    [SerializeField] private float _deathCutsceneDuration = 7.5f;

    private SpawnPool<ExplosionAnimation> _explosionPool;
    private PlayerShooter _playerShooter;
    private Health _health;
    private PlayerMover _playerMover;
    private Vector3 _startScale;
    private bool _isControlEnabled = true;
    private bool _isDead;

    private void Awake()
    {
        _playerShooter = GetComponent<PlayerShooter>();
        _health = GetComponent<Health>();
        _playerMover = GetComponent<PlayerMover>();
        _explosionPool = new SpawnPool<ExplosionAnimation>(_explosionAnimation, PoolCapacity, PoolMaxSize);
        _startScale = transform.localScale;
    }

    private void OnEnable()
    {
        _ammoHitHandler.HitDetected += OnAmmoHit;
        _playerCollisionHandler.AsteroidBumped += OnAsteroidBumped;
        _playerCollisionHandler.EnemyBumped += OnEnemyBumped;
        _health.Died += Die;
        _input.Jumped += OnJumped;
        _input.Shot += OnShot;
    }

    private void OnDisable()
    {
        _ammoHitHandler.HitDetected -= OnAmmoHit;
        _playerCollisionHandler.AsteroidBumped -= OnAsteroidBumped;
        _playerCollisionHandler.EnemyBumped -= OnEnemyBumped;
        _health.Died -= Die;
        _input.Jumped -= OnJumped;
        _input.Shot -= OnShot;
    }

    public float DeathDuration => _deathCutsceneDuration;

    public bool IsControlEnabled => _isControlEnabled;

    public event Action Died;
    public event Action AsteroidBumped;
    public event Action EnemyBumped;

    private void OnJumped()
    {
        if (_isControlEnabled == false)
            return;

        _playerMover.Jump();
    }

    private void OnShot()
    {
        if (_isControlEnabled == false)
            return;

        _playerShooter.Shoot();
    }

    public void UnlockInput()
    {
        _isControlEnabled = true;
    }

    public void LockInput()
    {
        _isControlEnabled = false;
    }

    public void EnableControl()
    {
        UnlockInput();
        _playerMover.enabled = true;
        _playerMover.Resume();
        _bodyCollider.enabled = true;
    }

    public void DisableControl()
    {
        LockInput();
        _playerMover.Stop();
        _playerMover.enabled = false;
        _bodyCollider.enabled = false;
    }

    public void ReturnToStart()
    {
        _playerMover.ReturnToStart();
    }

    public void TakeDamage(int damage)
    {
        _health.TakeDamage(damage);
    }

    private void OnAmmoHit(Ammo ammo)
    {
        TakeDamage(ammo.Damage);
    }

    private void OnAsteroidBumped(Asteroid asteroid)
    {
        TakeDamage(asteroid.Damage);

        if (_isDead)
            return;

        PushOff();
        AsteroidBumped?.Invoke();
    }

    private void OnEnemyBumped(EnemyShip enemyShip)
    {
        TakeDamage(enemyShip.BodyDamage);

        if (_isDead)
            return;

        PushOff();
        EnemyBumped?.Invoke();
    }

    public void Die()
    {
        if (_isDead)
            return;

        _isDead = true;
        _health.TakeDamage(_health.Value);

        ExplosionAnimation explosion = _explosionPool.Get(transform.position);

        explosion.Play();

        DisableControl();
        _shipSprite.enabled = false;

        Died?.Invoke();
    }

    public void Restart()
    {
        _isDead = false;
        _shipSprite.enabled = true;
        transform.localScale = _startScale;
        _playerShooter.ReleaseAll();
        _explosionPool.ReleaseAll();
        _playerCollisionHandler.ReleaseAll();
        _health.Init();
        EnableControl();
        ReturnToStart();
    }

    public void Remove()
    {
        Die();
    }

    private void PushOff()
    {
        _playerMover.Push();
    }
}
