using System;
using System.Collections.Generic;
using UnityEngine;

public class PoolRegistry
{
    private readonly Dictionary<UnityEngine.Object, ISpawnPool> _pools =
        new Dictionary<UnityEngine.Object, ISpawnPool>();

    public SpawnPool<T> GetPool<T>(T prefab, Action<T> created = null)
        where T : MonoBehaviour, IPoolable
    {
        if (_pools.TryGetValue(prefab, out ISpawnPool existing))
            return (SpawnPool<T>)existing;

        SpawnPool<T> pool = new SpawnPool<T>(prefab, created: created);

        _pools.Add(prefab, pool);

        return pool;
    }

    public void ReleaseAll()
    {
        foreach (ISpawnPool pool in _pools.Values)
            pool.ReleaseAll();
    }
}
