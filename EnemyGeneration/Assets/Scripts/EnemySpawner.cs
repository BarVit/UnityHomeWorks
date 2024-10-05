using UnityEngine;
using UnityEngine.Pool;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Enemy _enemy;

    private SpawnPointGenerator _spawnPointGenerator;

    private ObjectPool<Enemy> _pool;

    private int _poolCapacity = 10;
    private int _poolMaxSize = 100;

    private float _spawnRate = 2f;

    private void Awake()
    {
        _spawnPointGenerator = GetComponent<SpawnPointGenerator>();

        _pool = new ObjectPool<Enemy>(
            createFunc: () => Instantiate(_enemy),
            actionOnGet: (obj) => InitEnemy(obj),
            actionOnRelease: (obj) => obj.gameObject.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj.gameObject),
            collectionCheck: true,
            defaultCapacity: _poolCapacity,
            maxSize: _poolMaxSize);
    }

    private void Start()
    {
        StartCoroutine(SpawnEnemy());
    }

    private IEnumerator SpawnEnemy()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(_spawnRate);

        while (true)
        {
            _pool.Get();
            yield return waitForSeconds;
        }
    }

    private void InitEnemy(Enemy enemy)
    {
        Vector3 direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));

        enemy.Init(direction);
        enemy.transform.position = GetSpawnPosition();
        enemy.gameObject.SetActive(true);
    }

    private Vector3 GetSpawnPosition()
    {
        return _spawnPointGenerator.GetRandomWaypoint();
    }
}