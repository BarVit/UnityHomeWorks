using System;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(AudioSource), typeof(BulletMover))]
public class Ammo : MonoBehaviour, IPoolable, IRemoveable
{
    private const float HitEffectLifetime = 0.5f;

    [FormerlySerializedAs("_prefab")]
    [SerializeField] private ExplosionAnimation _hitEffectPrefab;
    [SerializeField] private AudioSource _audioShot;

    private BulletMover _bulletMover;
    private SpawnPool<ExplosionAnimation> _hitEffectPool;

    [field: SerializeField] public int Damage { get; private set; }

    public ExplosionAnimation HitEffectPrefab => _hitEffectPrefab;

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

    public void PlayShotSound()
    {
        _audioShot.Play();
    }

    public void Hit(Vector2 point)
    {
        ExplosionAnimation hitEffect = _hitEffectPool.Get(point);

        hitEffect.Play(HitEffectLifetime);
    }
}
