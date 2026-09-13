using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class SpawnPool<T> where T : MonoBehaviour, IPoolable
{
    private readonly T _prefab;
    private readonly Action<T> _created;
    private readonly Action<IPoolable> _release;
    private readonly ObjectPool<T> _pool;
    private readonly List<T> _active = new();

    public SpawnPool(T prefab, int capacity = 10, int maxSize = 100, Action<T> created = null)
    {
        _prefab = prefab;
        _created = created;
        _release = Release;

        _pool = new ObjectPool<T>(
            createFunc: Create,
            actionOnGet: PrepareForUse,
            actionOnRelease: PrepareForStorage,
            actionOnDestroy: DestroyItem,
            collectionCheck: true,
            defaultCapacity: capacity,
            maxSize: maxSize);
    }

    public T Get(Vector3 position)
    {
        T item = _pool.Get();

        item.transform.position = position;

        return Activate(item);
    }

    public T Get(Vector3 position, Quaternion rotation)
    {
        T item = _pool.Get();

        item.transform.SetPositionAndRotation(position, rotation);

        return Activate(item);
    }

    public void ReleaseAll()
    {
        for (int i = _active.Count - 1; i >= 0; i--)
            Release(_active[i]);
    }

    private T Activate(T item)
    {
        _active.Add(item);
        item.gameObject.SetActive(true);
        item.OnSpawn();

        return item;
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
        item.Released += _release;
    }

    private void PrepareForStorage(T item)
    {
        item.OnDespawn();
        item.gameObject.SetActive(false);
    }

    private void Release(IPoolable item)
    {
        item.Released -= _release;
        _active.Remove((T)item);
        _pool.Release((T)item);
    }

    private void DestroyItem(T item)
    {
        UnityEngine.Object.Destroy(item.gameObject);
    }
}
