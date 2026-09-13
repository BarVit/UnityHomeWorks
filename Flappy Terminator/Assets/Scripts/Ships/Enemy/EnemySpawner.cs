using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyShip _prefab;
    [SerializeField] private ExplosionAnimation _explosionPrefab;
    [SerializeField] private EnemyDeadBody _deadBodyPrefab;
    [SerializeField] private Ammo _ammoPrefab;
    [SerializeField] private float _spawnDelay;
    [SerializeField] private float _spawnMargin = 1f;
    [SerializeField] private float _minSpawnPositionY = -3f;
    [SerializeField] private float _maxSpawnPositionY = 3f;

    private Camera _camera;
    private SpawnPool<EnemyShip> _pool;
    private SpawnPool<ExplosionAnimation> _explosionPool;
    private SpawnPool<EnemyDeadBody> _deadBodyPool;
    private SpawnPool<Ammo> _ammoPool;
    private SpawnPool<ExplosionAnimation> _hitEffectPool;
    private Coroutine _spawning;
    private int _waveSize;
    private int _retiredCount;
    private bool _isRestarting;

    public event Action<EnemyShip> EnemyDied;
    public event Action WaveCleared;

    private void Awake()
    {
        _camera = Camera.main;

        _explosionPool = new SpawnPool<ExplosionAnimation>(_explosionPrefab);
        _deadBodyPool = new SpawnPool<EnemyDeadBody>(_deadBodyPrefab);
        _ammoPool = new SpawnPool<Ammo>(_ammoPrefab);
        _hitEffectPool = new SpawnPool<ExplosionAnimation>(_ammoPrefab.HitEffectPrefab);
        _pool = new SpawnPool<EnemyShip>(_prefab, created: OnEnemyCreated);
    }

    public void StartWave(int waveSize)
    {
        StopLevel();

        _hitEffectPool.ReleaseAll();
        _explosionPool.ReleaseAll();
        _deadBodyPool.ReleaseAll();

        _waveSize = waveSize;
        _retiredCount = 0;

        _spawning = StartCoroutine(Spawn());
    }

    public void StopLevel()
    {
        StopSpawn();

        _isRestarting = true;
        _pool.ReleaseAll();
        _isRestarting = false;

        _ammoPool.ReleaseAll();
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
        ship.Init(_explosionPool, _deadBodyPool, _ammoPool, _hitEffectPool);
        ship.Died += OnEnemyDied;
        ship.Retired += OnEnemyRetired;
    }

    private void OnEnemyDied(EnemyShip ship)
    {
        EnemyDied?.Invoke(ship);
    }

    private void OnEnemyRetired(EnemyShip ship)
    {
        if (_isRestarting)
            return;

        _retiredCount++;

        if (_retiredCount >= _waveSize)
            WaveCleared?.Invoke();
    }

    private IEnumerator Spawn()
    {
        WaitForSeconds spawnDelay = new(_spawnDelay);

        for (int i = 0; i < _waveSize; i++)
        {
            _pool.Get(new Vector3(
                ScreenArea.GetRightEdge(_camera) + _spawnMargin,
                Random.Range(_minSpawnPositionY, _maxSpawnPositionY),
                0));

            yield return spawnDelay;
        }

        _spawning = null;
    }
}
