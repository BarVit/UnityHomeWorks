using System;
using UnityEngine;

[RequireComponent(typeof(PlayerShooter), typeof(Health))]
[RequireComponent(typeof(PlayerMover), typeof(PlayerInputRouter))]
public class PlayerShip : MonoBehaviour, IRemoveable
{
    [SerializeField] private AmmoHitHandler _ammoHitHandler;
    [SerializeField] private PlayerCollisionHandler _playerCollisionHandler;
    [SerializeField] private Collider2D _bodyCollider;
    [SerializeField] private SpriteRenderer _shipSprite;
    [SerializeField] private ShipExplosion _explosion;
    [SerializeField] private float _deathCutsceneDuration = 7.5f;

    private PlayerInputRouter _inputRouter;
    private PlayerShooter _playerShooter;
    private Health _health;
    private PlayerMover _playerMover;
    private Vector3 _startScale;
    private bool _isDead;

    private void Awake()
    {
        _playerShooter = GetComponent<PlayerShooter>();
        _health = GetComponent<Health>();
        _playerMover = GetComponent<PlayerMover>();
        _inputRouter = GetComponent<PlayerInputRouter>();
        _startScale = transform.localScale;
    }

    private void OnEnable()
    {
        _ammoHitHandler.HitDetected += OnAmmoHit;
        _playerCollisionHandler.AsteroidBumped += OnAsteroidBumped;
        _playerCollisionHandler.EnemyBumped += OnEnemyBumped;
        _health.Died += Die;
    }

    private void OnDisable()
    {
        _ammoHitHandler.HitDetected -= OnAmmoHit;
        _playerCollisionHandler.AsteroidBumped -= OnAsteroidBumped;
        _playerCollisionHandler.EnemyBumped -= OnEnemyBumped;
        _health.Died -= Die;
    }

    public float DeathDuration => _deathCutsceneDuration;

    public bool IsControlEnabled => _inputRouter.IsEnabled;

    public event Action Died;
    public event Action AsteroidBumped;
    public event Action EnemyBumped;

    public void UnlockInput()
    {
        _inputRouter.Unlock();
    }

    public void LockInput()
    {
        _inputRouter.Lock();
    }

    private void EnableControl()
    {
        UnlockInput();
        _playerMover.enabled = true;
        _playerMover.Resume();
        _bodyCollider.enabled = true;
    }

    public void Hide()
    {
        _shipSprite.enabled = false;
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

    private void TakeDamage(int damage)
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

    private void Die()
    {
        if (_isDead)
            return;

        _isDead = true;
        _health.TakeDamage(_health.Value);

        _explosion.Play(transform.position);

        DisableControl();
        Hide();

        Died?.Invoke();
    }

    public void Restart()
    {
        _isDead = false;
        _shipSprite.enabled = true;
        transform.localScale = _startScale;
        _playerShooter.ReleaseAll();
        _explosion.ReleaseAll();
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
