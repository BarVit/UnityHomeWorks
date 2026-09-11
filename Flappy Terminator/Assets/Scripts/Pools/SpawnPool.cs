using System;
using UnityEngine;
using UnityEngine.Pool;

public class SpawnPool<T> where T : MonoBehaviour, IPoolable
{
    private readonly T _prefab;
    private readonly Action<T> _created;
    private readonly ObjectPool<T> _pool;

    public SpawnPool(T prefab, int capacity = 10, int maxSize = 100, Action<T> created = null)
    {
        _prefab = prefab;
        _created = created;

        _pool = new ObjectPool<T>(
            createFunc: Create,
            actionOnGet: PrepareForUse,
            actionOnRelease: PrepareForStorage,
            actionOnDestroy: DestroyItem,
            collectionCheck: true,
            defaultCapacity: capacity,
            maxSize: maxSize);
    }

    public T Get()
    {
        return _pool.Get();
    }

    private T Create()
    {
        T item = UnityEngine.Object.Instantiate(_prefab);

        item.gameObject.SetActive(false);
        _created?.Invoke(item);

        return item;
    }

    private void PrepareForUse(T item)
    {
        item.Released += Release;
        item.gameObject.SetActive(true);
        item.OnSpawn();
    }

    private void PrepareForStorage(T item)
    {
        item.OnDespawn();
        item.gameObject.SetActive(false);
    }

    private void Release(IPoolable item)
    {
        item.Released -= Release;
        _pool.Release((T)item);
    }

    private void DestroyItem(T item)
    {
        UnityEngine.Object.Destroy(item.gameObject);
    }
}
