using System;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(BulletMover))]
public class Ammo : MonoBehaviour, IPoolable, IRemoveable
{
    [FormerlySerializedAs("_prefab")]
    [SerializeField] private ExplosionAnimation _hitEffectPrefab;
    [SerializeField] private Sound _shotSound;

    private BulletMover _bulletMover;
    private SpawnPool<ExplosionAnimation> _hitEffectPool;

    [field: SerializeField] public int Damage { get; private set; }

    public ExplosionAnimation HitEffectPrefab => _hitEffectPrefab;
    public Sound ShotSound => _shotSound;

    public event Action<IPoolable> Released;

    private void Awake()
    {
        _bulletMover = GetComponent<BulletMover>();
    }

    public void Init(SpawnPool<ExplosionAnimation> hitEffectPool)
    {
        _hitEffectPool = hitEffectPool;
    }

    public void OnSpawn()
    {
    }

    public void OnDespawn()
    {
        _bulletMover.Stop();
    }

    public void Remove()
    {
        Released?.Invoke(this);
    }

    public void Fly()
    {
        _bulletMover.Fly();
    }

    public void Hit(Vector2 point)
    {
        ExplosionAnimation hitEffect = _hitEffectPool.Get(point);

        hitEffect.Play();
    }
}
