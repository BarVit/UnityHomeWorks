using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyShip _prefab;
    [SerializeField] private int _waveSize = 10;
    [SerializeField] private float _spawnDelay;
    [SerializeField] private float _spawnPositionX = 9f;
    [SerializeField] private float _minSpawnPositionY = -3f;
    [SerializeField] private float _maxSpawnPositionY = 3f;

    private SpawnPool<EnemyShip> _pool;
    private SpawnPool<ExplosionAnimation> _explosionPool;
    private SpawnPool<Ammo> _ammoPool;
    private SpawnPool<ExplosionAnimation> _hitEffectPool;
    private Coroutine _spawning;
    private int _retiredCount;

    public event Action<EnemyShip> EnemyDied;
    public event Action WaveCleared;

    private void Awake()
    {
        Ammo ammoPrefab = _prefab.GetComponent<EnemyShooter>().AmmoPrefab;

        _explosionPool = new SpawnPool<ExplosionAnimation>(_prefab.ExplosionPrefab);
        _ammoPool = new SpawnPool<Ammo>(ammoPrefab);
        _hitEffectPool = new SpawnPool<ExplosionAnimation>(ammoPrefab.HitEffectPrefab);
        _pool = new SpawnPool<EnemyShip>(_prefab, created: OnEnemyCreated);
    }

    private void Start()
    {
        _spawning = StartCoroutine(Spawn());
    }

    public void StopSpawn()
    {
        if (_spawning == null)
            return;

        StopCoroutine(_spawning);
        _spawning = null;
    }

    private void OnEnemyCreated(EnemyShip ship)
    {
        ship.Init(_explosionPool, _ammoPool, _hitEffectPool);
        ship.Died += OnEnemyDied;
        ship.Retired += OnEnemyRetired;
    }

    private void OnEnemyDied(EnemyShip ship)
    {
        EnemyDied?.Invoke(ship);
    }

    private void OnEnemyRetired(EnemyShip ship)
    {
        _retiredCount++;

        if (_retiredCount == _waveSize)
            WaveCleared?.Invoke();
    }

    private IEnumerator Spawn()
    {
        WaitForSeconds spawnDelay = new(_spawnDelay);

        for (int i = 0; i < _waveSize; i++)
        {
            _pool.Get(new Vector3(
                _spawnPositionX,
                Random.Range(_minSpawnPositionY, _maxSpawnPositionY),
                0));

            yield return spawnDelay;
        }

        _spawning = null;
    }
}
