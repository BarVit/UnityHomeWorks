using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyShipTable _enemyShips;
    [SerializeField] private EnemyTierTable _enemyTiers;
    [SerializeField] private WeaponTable _weapons;
    [SerializeField] private LocationTable _locations;
    [SerializeField] private SystemTable _systems;
    [SerializeField] private ExplosionAnimation _explosionPrefab;
    [SerializeField] private EnemyDeadBody _deadBodyPrefab;
    [SerializeField] private AudioSource _hitAudioSource;
    [SerializeField] private LayerMask _targetLayers;
    [SerializeField] private float _spawnDelay;
    [SerializeField] private float _spawnMargin = 1f;
    [SerializeField] private float _minSpawnPositionY = -3f;
    [SerializeField] private float _maxSpawnPositionY = 3f;

    private readonly PoolRegistry _pools = new PoolRegistry("Enemies");
    private readonly List<SpawnPool<EnemyShip>> _shipPools = new List<SpawnPool<EnemyShip>>();
    private readonly List<SpawnPool<Ammo>> _ammoPools = new List<SpawnPool<Ammo>>();

    private Camera _camera;
    private EnemyContext _context;
    private EnemyRoster _roster;
    private Action<EnemyShip> _created;
    private Coroutine _spawning;
    private Wave _wave;

    public event Action<EnemyShip> EnemyDied;
    public event Action WaveCleared;

    private void Awake()
    {
        _camera = Camera.main;
        _created = OnEnemyCreated;
        _context = new EnemyContext(_pools, _explosionPrefab, _deadBodyPrefab,
            new ShotAudio(_hitAudioSource, _camera), _targetLayers, _camera);
        _roster = new EnemyRoster(_enemyShips, _enemyTiers, _weapons, _locations, _systems);
    }

    public void StartWave(int waveSize, string locationId)
    {
        Open(locationId);
        ClearField();

        _wave = new Wave(waveSize);
        _spawning = StartCoroutine(Spawn());
    }

    public void FinishWave()
    {
        EndWave();

        foreach (SpawnPool<Ammo> pool in _ammoPools)
            pool.ForEachActive(ammo => ammo.Disarm());
    }

    public void StopSpawn()
    {
        if (_spawning == null)
            return;

        StopCoroutine(_spawning);
        _spawning = null;
    }

    private void Open(string locationId)
    {
        _roster.Open(locationId);

        if (_roster.Available.Count == 0)
            throw new InvalidOperationException(
                "EnemySpawner: для локации " + locationId + " в таблицах нет ни одного врага");

        _shipPools.Clear();
        _ammoPools.Clear();

        foreach (EnemySetup setup in _roster.Available)
        {
            Collect(_shipPools, _pools.GetPool(setup.Prefab, _created));

            if (setup.Weapon != null && setup.Weapon.Projectile != null)
                Collect(_ammoPools, _pools.GetPool(setup.Weapon.Projectile));
        }
    }

    private void Collect<T>(List<SpawnPool<T>> pools, SpawnPool<T> pool)
        where T : MonoBehaviour, IPoolable
    {
        if (pools.Contains(pool) == false)
            pools.Add(pool);
    }

    private void ClearField()
    {
        EndWave();
        _pools.ReleaseAll();
    }

    private void EndWave()
    {
        StopSpawn();

        _wave = null;

        foreach (SpawnPool<EnemyShip> pool in _shipPools)
            pool.ReleaseAll();
    }

    private void OnEnemyCreated(EnemyShip ship)
    {
        ship.Init(_context);
        ship.Died += OnEnemyDied;
        ship.Retired += OnEnemyRetired;
    }

    private void OnEnemyDied(EnemyShip ship)
    {
        EnemyDied?.Invoke(ship);
    }

    private void OnEnemyRetired(EnemyShip ship)
    {
        if (_wave == null)
            return;

        _wave.CountRetired();

        if (_wave.IsCleared)
            WaveCleared?.Invoke();
    }

    private IEnumerator Spawn()
    {
        WaitForSeconds spawnDelay = new(_spawnDelay);

        for (int i = 0; i < _wave.Size; i++)
        {
            Release(_roster.GetRandom());

            yield return spawnDelay;
        }

        _spawning = null;
    }

    public void Spawn(EnemySetup setup)
    {
        if (setup.Weapon != null && setup.Weapon.Projectile != null)
            Collect(_ammoPools, _pools.GetPool(setup.Weapon.Projectile));

        Release(setup);
    }

    private void Release(EnemySetup setup)
    {
        SpawnPool<EnemyShip> pool = _pools.GetPool(setup.Prefab, _created);
        EnemyShip ship = pool.Get(
            setup.Movement.GetSpawnPoint(_camera, _spawnMargin, _minSpawnPositionY, _maxSpawnPositionY));

        ship.Launch(setup);
    }
}
