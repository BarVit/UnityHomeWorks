using System;
using UnityEngine;

[RequireComponent(typeof(Health), typeof(EnemyMover), typeof(EnemyShooter))]
public class EnemyShip : MonoBehaviour, IPoolable, IRemoveable
{
    [SerializeField] private AmmoHitHandler _ammoHitHandler;
    [SerializeField] private ExplosionAnimation _explosion;
    [SerializeField] private EnemyDeadBody _deadBody;
    [SerializeField] private float _exitMargin = 1f;

    private Camera _camera;
    private Health _health;
    private EnemyMover _mover;
    private EnemyShooter _shooter;
    private SpawnPool<ExplosionAnimation> _explosionPool;
    private SpawnPool<EnemyDeadBody> _deadBodyPool;
    private bool _isDead;

    [field: SerializeField] public int BodyDamage { get; private set; }

    public ExplosionAnimation ExplosionPrefab => _explosion;
    public EnemyDeadBody DeadBodyPrefab => _deadBody;

    public event Action<IPoolable> Released;
    public event Action<EnemyShip> Died;
    public event Action<EnemyShip> Retired;

    private void Awake()
    {
        _camera = Camera.main;
        _health = GetComponent<Health>();
        _mover = GetComponent<EnemyMover>();
        _shooter = GetComponent<EnemyShooter>();
    }

    private void OnEnable()
    {
        _ammoHitHandler.HitDetected += OnAmmoHit;
        _health.Died += Die;
    }

    private void OnDisable()
    {
        _ammoHitHandler.HitDetected -= OnAmmoHit;
        _health.Died -= Die;
    }

    private void Update()
    {
        if (transform.position.x + _exitMargin < ScreenArea.GetLeftEdge(_camera))
            Remove();
    }

    public void Init(
        SpawnPool<ExplosionAnimation> explosionPool,
        SpawnPool<EnemyDeadBody> deadBodyPool,
        SpawnPool<Ammo> ammoPool,
        SpawnPool<ExplosionAnimation> hitEffectPool)
    {
        _explosionPool = explosionPool;
        _deadBodyPool = deadBodyPool;
        _shooter.Init(ammoPool, hitEffectPool);
    }

    public void OnSpawn()
    {
        _isDead = false;
        _health.Init();
        _mover.Fly();
        _shooter.StartShoot();
    }

    public void OnDespawn()
    {
        _shooter.StopShoot();
        _mover.Stop();
        Retired?.Invoke(this);
    }

    public void Remove()
    {
        Released?.Invoke(this);
    }

    private void TakeDamage(int damage)
    {
        _health.TakeDamage(damage);
    }

    private void OnAmmoHit(Ammo ammo)
    {
        TakeDamage(ammo.Damage);
    }

    public void Die()
    {
        if (_isDead)
            return;

        _isDead = true;

        EnemyDeadBody deadBody = _deadBodyPool.Get(transform.position);

        deadBody.End();

        ExplosionAnimation explosion = _explosionPool.Get(transform.position);

        explosion.Play();

        Released?.Invoke(this);
        Died?.Invoke(this);
    }
}
