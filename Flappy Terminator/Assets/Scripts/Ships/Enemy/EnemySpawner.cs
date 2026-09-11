using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyShip _prefab;
    [SerializeField] private float _spawnDelay;
    [SerializeField] private float _spawnPositionX = 9f;
    [SerializeField] private float _minSpawnPositionY = -3f;
    [SerializeField] private float _maxSpawnPositionY = 3f;

    private SpawnPool<EnemyShip> _pool;
    private SpawnPool<ExplosionAnimation> _explosionPool;
    private Coroutine _spawning;

    public event Action<EnemyShip> EnemyDied;

    private void Awake()
    {
        _explosionPool = new SpawnPool<ExplosionAnimation>(_prefab.ExplosionPrefab);
        _pool = new SpawnPool<EnemyShip>(_prefab, created: OnEnemyCreated);
    }

    private void Start()
    {
        _spawning = StartCoroutine(Spawn());
    }

    private void OnEnemyCreated(EnemyShip ship)
    {
        ship.Init(_explosionPool);
        ship.Died += OnEnemyDied;
    }

    private void OnEnemyDied(EnemyShip ship)
    {
        EnemyDied?.Invoke(ship);
    }

    private IEnumerator Spawn()
    {
        WaitForSeconds spawnDelay = new(_spawnDelay);

        while (enabled)
        {
            EnemyShip ship = _pool.Get();

            ship.transform.position = new Vector3(
                _spawnPositionX,
                Random.Range(_minSpawnPositionY, _maxSpawnPositionY),
                0);

            yield return spawnDelay;
        }
    }
}
