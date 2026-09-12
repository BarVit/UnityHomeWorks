using System;
using UnityEngine;

[RequireComponent(typeof(Health), typeof(EnemyMover), typeof(EnemyShooter))]
public class EnemyShip : MonoBehaviour, IPoolable, IRemoveable, IDamageable
{
    private const float ExplosionLifetime = 1f;

    [SerializeField] private AmmoHitHandler _ammoHitHandler;
    [SerializeField] private ExplosionAnimation _explosion;
    [SerializeField] private EnemyDeadBody _deadBody;

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
        _health = GetComponent<Health>();
        _mover = GetComponent<EnemyMover>();
        _shooter = GetComponent<EnemyShooter>();
    }

    private void OnEnable()
    {
        _ammoHitHandler.HitDetected += TakeDamage;
        _health.Died += Die;
    }

    private void OnDisable()
    {
        _ammoHitHandler.HitDetected -= TakeDamage;
        _health.Died -= Die;
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

    public void TakeDamage(Ammo ammo)
    {
        _health.TakeDamage(ammo.Damage);
    }

    public void Die()
    {
        if (_isDead)
            return;

        _isDead = true;

        EnemyDeadBody deadBody = _deadBodyPool.Get(transform.position);

        deadBody.End();

        ExplosionAnimation explosion = _explosionPool.Get(transform.position);

        explosion.Play(ExplosionLifetime);

        Released?.Invoke(this);
        Died?.Invoke(this);
    }
}
