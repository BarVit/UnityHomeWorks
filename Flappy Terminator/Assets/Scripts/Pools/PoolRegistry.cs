using System.Collections.Generic;
using UnityEngine;

public class PoolRegistry
{
    private readonly Dictionary<Object, ISpawnPool> _pools = new Dictionary<Object, ISpawnPool>();

    public SpawnPool<T> GetPool<T>(T prefab) where T : MonoBehaviour, IPoolable
    {
        if (_pools.TryGetValue(prefab, out ISpawnPool existing))
            return (SpawnPool<T>)existing;

        SpawnPool<T> created = new SpawnPool<T>(prefab);

        _pools.Add(prefab, created);

        return created;
    }

    public void ReleaseAll()
    {
        foreach (ISpawnPool pool in _pools.Values)
            pool.ReleaseAll();
    }
}
