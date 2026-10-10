using System;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(BulletMover))]
public class Ammo : MonoBehaviour, IPoolable, IRemoveable
{
    [FormerlySerializedAs("_prefab")]
    [SerializeField] private ExplosionAnimation _hitEffectPrefab;
    [SerializeField] private Sound _shotSound;
    [SerializeField] private Sound _hitSound;
    [SerializeField] private Sound _worldHitSound;

    private BulletMover _bulletMover;
    private SpawnPool<ExplosionAnimation> _hitEffectPool;

    [field: SerializeField] public int Damage { get; private set; }

    public ExplosionAnimation HitEffectPrefab => _hitEffectPrefab;
    public Sound ShotSound => _shotSound;

    public Sound HitSound => _hitSound;

    public Sound WorldHitSound => _worldHitSound;

    public GameObject StruckObject { get; private set; }

    public bool IsCrit { get; private set; }

    public bool IsArmed { get; private set; }

    public bool IsPersistent { get; private set; }

    public event Action<IPoolable> Released;

    public event Action<Ammo> Struck;

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
        IsArmed = true;
        IsPersistent = false;
        StruckObject = null;
        Struck = null;
    }

    public void MakePersistent()
    {
        IsPersistent = true;
    }

    public void Disarm()
    {
        IsArmed = false;
    }

    public void OnDespawn()
    {
        _bulletMover.Stop();
    }

    public void Remove()
    {
        Released?.Invoke(this);
    }

    public void SetDamage(int damage, bool isCrit = false)
    {
        Damage = damage;
        IsCrit = isCrit;
    }

    public void Fly()
    {
        _bulletMover.Fly();
    }

    public void Fly(float speed)
    {
        _bulletMover.Fly(speed);
    }

    public void Stop()
    {
        _bulletMover.Stop();
    }

    public void Hit(Vector2 point, GameObject struck)
    {
        StruckObject = struck;
        Struck?.Invoke(this);

        if (_hitEffectPool == null)
            return;

        ExplosionAnimation hitEffect = _hitEffectPool.Get(point, transform.rotation);

        hitEffect.Play();
    }
}
