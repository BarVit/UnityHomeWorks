using System;
using UnityEngine;

[RequireComponent(typeof(PlayerShooter), typeof(Health))]
[RequireComponent(typeof(PlayerMover), typeof(PlayerInputRouter))]
public class PlayerShip : MonoBehaviour, IRemoveable, IDamageable
{
    [SerializeField] private AmmoHitHandler _ammoHitHandler;
    [SerializeField] private AmmoHitHandler _shieldHitHandler;
    [SerializeField] private PlayerCollisionHandler _playerCollisionHandler;
    [SerializeField] private Collider2D _bodyCollider;
    [SerializeField] private SpriteRenderer _shipSprite;
    [SerializeField] private ShipExplosion _explosion;
    [SerializeField] private float _deathCutsceneDuration = 7.5f;
    [SerializeField] private EnergyShield _shield;

    private PlayerInputRouter _inputRouter;
    private PlayerShooter _playerShooter;
    private Health _health;
    private PlayerMover _playerMover;
    private Vector3 _startScale;
    private int _baseMaxHealth;
    private int _healthBonus;
    private bool _isDead;

    private void Awake()
    {
        _playerShooter = GetComponent<PlayerShooter>();
        _health = GetComponent<Health>();
        _playerMover = GetComponent<PlayerMover>();
        _inputRouter = GetComponent<PlayerInputRouter>();
        _startScale = transform.localScale;
        _baseMaxHealth = _health.MaxValue;
    }

    private void OnEnable()
    {
        _ammoHitHandler.HitDetected += OnAmmoHit;
        _shieldHitHandler.HitDetected += OnAmmoHit;
        _playerCollisionHandler.AsteroidBumped += OnAsteroidBumped;
        _playerCollisionHandler.EnemyBumped += OnEnemyBumped;
        _health.Died += Die;
    }

    private void OnDisable()
    {
        _ammoHitHandler.HitDetected -= OnAmmoHit;
        _shieldHitHandler.HitDetected -= OnAmmoHit;
        _playerCollisionHandler.AsteroidBumped -= OnAsteroidBumped;
        _playerCollisionHandler.EnemyBumped -= OnEnemyBumped;
        _health.Died -= Die;
    }

    public float DeathDuration => _deathCutsceneDuration;

    public PlayerShooter Shooter => _playerShooter;

    public bool IsControlEnabled => _inputRouter.IsEnabled;

    public event Action Died;
    public event Action<int, bool> Damaged;
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

    public bool IsVulnerable => true;

    public void TakeDamage(int damage)
    {
        ApplyDamage(damage, false);
    }

    public int BaseMaxHealth => _baseMaxHealth;

    public void SetHealthBonus(int bonus)
    {
        _healthBonus = Mathf.Max(0, bonus);
    }

    private void ApplyDamage(int damage, bool isEnvironment)
    {
        damage = _shield.Absorb(damage);

        if (damage <= 0)
            return;

        int before = _health.Value;

        _health.TakeDamage(damage);

        int lost = before - _health.Value;

        if (lost > 0)
            Damaged?.Invoke(lost, isEnvironment);
    }

    private void OnAmmoHit(Ammo ammo)
    {
        TakeDamage(ammo.Damage);
    }

    private void OnAsteroidBumped(Asteroid asteroid)
    {
        ApplyDamage(asteroid.Damage, true);

        if (_isDead)
            return;

        PushOff();
        AsteroidBumped?.Invoke();
    }

    private void OnEnemyBumped(EnemyShip enemyShip)
    {
        ApplyDamage(enemyShip.BodyDamage, true);

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
        _health.Init(_baseMaxHealth + _healthBonus);
        EnableControl();
        ReturnToStart();
    }

    public void Remove()
    {
        if (_isDead == false)
            ApplyDamage(_health.Value, true);

        Die();
    }

    private void PushOff()
    {
        _playerMover.Push();
    }
}
