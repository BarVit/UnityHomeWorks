using UnityEngine;
using UnityEngine.Pool;

public class EnemyPool : MonoBehaviour
{
    private ObjectPool<Enemy> _pool;
    private Enemy _enemy;

    private int _poolCapacity = 10;
    private int _poolMaxSize = 100;

    private void Awake()
    {
        _pool = new ObjectPool<Enemy>(
            createFunc: () => Instantiate(_enemy),
            actionOnGet: (obj) => obj.gameObject.SetActive(true),
            actionOnRelease: (obj) => obj.gameObject.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj.gameObject),
            collectionCheck: true,
            defaultCapacity: _poolCapacity,
            maxSize: _poolMaxSize);
    }

    public Enemy GetEnemy(Enemy prefab)
    {
        Enemy enemy;

        _enemy = prefab;
        enemy = _pool.Get();
        enemy.Init();
        return enemy;
    }
}