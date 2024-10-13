using UnityEngine;
using UnityEngine.Pool;
using System.Collections;

public class EnemyPool<T> : MonoBehaviour where T : Enemy
{
    private T _enemy;

    private ObjectPool<T> _pool;

    private int _poolCapacity = 10;
    private int _poolMaxSize = 100;

    //private float _spawnRate = 2f;
    public T NextEnemy => _pool.Get();

    private void Awake()
    {
        _pool = new ObjectPool<T>(
            createFunc: () => Instantiate(_enemy),
            actionOnGet: (obj) => InitEnemy(obj),
            actionOnRelease: (obj) => obj.gameObject.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj.gameObject),
            collectionCheck: true,
            defaultCapacity: _poolCapacity,
            maxSize: _poolMaxSize);
    }

    //private void Start()
    //{
    //    StartCoroutine(SpawnEnemy());
    //}

    //private IEnumerator SpawnEnemy()
    //{
    //    WaitForSeconds waitForSeconds = new WaitForSeconds(_spawnRate);

    //    while (true)
    //    {
    //        _pool.Get();
    //        yield return waitForSeconds;
    //    }
    //}

    private void InitEnemy(T t)
    {
        //t.Init(t.name);
        t.gameObject.SetActive(true);
    }
}