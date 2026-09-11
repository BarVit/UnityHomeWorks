using UnityEngine;
using UnityEngine.Pool;

public class ObjectPoolExplosion
{
    private ExplosionAnimation _prefab;
    private ObjectPool<ExplosionAnimation> _pool;
    private int _poolCapacity = 10;
    private int _poolMaxSize = 1000;

    public ObjectPoolExplosion(ExplosionAnimation prefab)
    {
        _prefab = prefab;
        InitializePool();
    }

    private void InitializePool()
    {
        _pool = new ObjectPool<ExplosionAnimation>(
            createFunc: () => Create(_prefab),
            actionOnGet: (ammo) => ammo.gameObject.SetActive(true),
            actionOnRelease: (ammo) => ammo.gameObject.SetActive(false),
            actionOnDestroy: (ammo) => GameObject.Destroy(ammo.gameObject),
            collectionCheck: true,
            defaultCapacity: _poolCapacity,
            maxSize: _poolMaxSize);
    }

    public ExplosionAnimation Get()
    {
        return _pool.Get();
    }

    public void Realese(ExplosionAnimation explosionAnimation)
    {
        _pool.Release(explosionAnimation);
    }

    private ExplosionAnimation Create(ExplosionAnimation prefab)
    {
        return GameObject.Instantiate(prefab);
    }
}