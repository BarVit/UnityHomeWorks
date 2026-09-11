using UnityEngine;
using UnityEngine.Pool;

public class ObjectPoolAsteroid
{
    private Asteroid _prefab;
    private ObjectPool<Asteroid> _pool;
    private int _poolCapacity = 100;
    private int _poolMaxSize = 10000;

    public ObjectPoolAsteroid(Asteroid prefab)
    {
        _prefab = prefab;
        InitializePool();
    }

    private void InitializePool()
    {
        _pool = new ObjectPool<Asteroid>(
            createFunc: () => Create(_prefab),
            actionOnGet: (asteroid) => asteroid.gameObject.SetActive(true),
            actionOnRelease: (asteroid) => asteroid.gameObject.SetActive(false),
            actionOnDestroy: (asteroid) => GameObject.Destroy(asteroid.gameObject),
            collectionCheck: true,
            defaultCapacity: _poolCapacity,
            maxSize: _poolMaxSize);
    }

    public Asteroid Get()
    {
        Asteroid asteroid = _pool.Get();

        asteroid.Bumped += Realese;

        return asteroid;
    }

    public void Realese(Asteroid asteroid)
    {
        asteroid.Bumped -= Realese;
        _pool.Release(asteroid);
    }

    private Asteroid Create(Asteroid prefab)
    {
        return GameObject.Instantiate(prefab);
    }
}