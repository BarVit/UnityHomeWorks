using System;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPoolEnemy
{
    private EnemyShip _prefab;
    private ObjectPool<EnemyShip> _pool;
    private int _poolCapacity = 10;
    private int _poolMaxSize = 100;
    private int _killsCount;

    public event Action<int> KillsCountChanged;

    public ObjectPoolEnemy(EnemyShip prefab)
    {
        _prefab = prefab;
        InitializePool();
    }

    private void InitializePool()
    {
        _pool = new ObjectPool<EnemyShip>(
            createFunc: () => Create(_prefab),
            actionOnGet: (ship) => ship.gameObject.SetActive(true),
            actionOnRelease: (ship) => ship.gameObject.SetActive(false),
            actionOnDestroy: (ship) => GameObject.Destroy(ship.gameObject),
            collectionCheck: true,
            defaultCapacity: _poolCapacity,
            maxSize: _poolMaxSize);
    }

    public EnemyShip Get()
    {
        EnemyShip ship = _pool.Get();

        ship.Bumped += Realese;
        ship.Died += CountKills;

        if(ship.TryGetComponent(out Health health))
        {
            health.Init();
        }

        return ship;
    }

    public void Realese(EnemyShip ship)
    {
        ship.Bumped -= Realese;
        ship.Died -= CountKills;

        if (ship.TryGetComponent(out EnemyMover enemyMover))
            enemyMover.Reset();

        if (ship.TryGetComponent(out EnemyShooter enemyShooter))
            enemyShooter.StopShoot();

        _pool.Release(ship);
    }

    private EnemyShip Create(EnemyShip prefab)
    {
        return GameObject.Instantiate(prefab);
    }

    private void CountKills(EnemyShip enemyShip)
    {
        _killsCount++;
        KillsCountChanged?.Invoke(_killsCount);
        Realese(enemyShip);
    }
}