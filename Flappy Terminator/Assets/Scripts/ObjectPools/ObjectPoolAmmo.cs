using UnityEngine;
using UnityEngine.Pool;

public class ObjectPoolAmmo
{
    private Ammo _prefab;
    private ObjectPool<Ammo> _pool;
    private int _poolCapacity = 10;
    private int _poolMaxSize = 1000;

    public ObjectPoolAmmo(Ammo prefab)
    {
        _prefab = prefab;
        InitializePool();
    }

    private void InitializePool()
    {
        _pool = new ObjectPool<Ammo>(
            createFunc: () => Create(_prefab),
            actionOnGet: (ammo) => ammo.gameObject.SetActive(true),
            actionOnRelease: (ammo) => ammo.gameObject.SetActive(false),
            actionOnDestroy: (ammo) => GameObject.Destroy(ammo.gameObject),
            collectionCheck: true,
            defaultCapacity: _poolCapacity,
            maxSize: _poolMaxSize);
    }

    public Ammo Get()
    {
        Ammo ammo = _pool.Get();

        ammo.Hitted += Realese;

        return ammo;
    }

    public void Realese(Ammo ammo)
    {
        if (ammo.TryGetComponent(out BulletMover bulletMover))
            bulletMover.Reset();

        ammo.Hitted -= Realese;
        _pool.Release(ammo);
    }

    private Ammo Create(Ammo prefab)
    {
        return GameObject.Instantiate(prefab);
    }
}