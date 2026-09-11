using System;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyShip : Ship, IRemoveable, IDamageable
{
    [SerializeField] private AmmoHitHandler _ammoHitHandler;
    [SerializeField] private ExplosionAnimation _explosion;
    [SerializeField] private EnemyDeadBody _deadBody;

    private Health _health;

    public event Action<EnemyShip> Bumped;
    public event Action<EnemyShip> Died;

    [field: SerializeField] public int BodyDamage { get; private set; }

    private void Awake()
    {
        _health = GetComponent<Health>();
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

    public void Remove()
    {
        Bumped?.Invoke(this);
    }

    public void TakeDamage(Ammo ammo)
    {
        _health.TakeDamage(ammo.Damage);
    }

    public void Die()
    {
        var deadBody = Instantiate(_deadBody, transform.position, Quaternion.identity);
        deadBody.End();
        var exp = Instantiate(_explosion, transform.position, Quaternion.identity);
        exp.End(0.5f);
        Died?.Invoke(this);
    }
}