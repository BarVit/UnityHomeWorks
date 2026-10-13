using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemySpawner : MonoBehaviour
{
    [FormerlySerializedAs("_enemyTiers")]
    [SerializeField] private EnemyTable _enemies;
    [SerializeField] private WeaponTable _weapons;
    [SerializeField] private BossTable _bosses;
    [SerializeField] private WaveTable _waves;
    [SerializeField] private ExplosionAnimation _explosionPrefab;
    [SerializeField] private EnemyDeadBody _deadBodyPrefab;
    [SerializeField] private AudioSource _hitAudioSource;
    [SerializeField] private LayerMask _targetLayers;
    [SerializeField] private float _spawnMargin = 1f;
    [SerializeField] private float _minSpawnPositionY = -3f;
    [SerializeField] private float _maxSpawnPositionY = 3f;
    [SerializeField] private float _announcementDuration = 2f;

    private readonly PoolRegistry _pools = new PoolRegistry("Enemies");
    private readonly List<SpawnPool<EnemyShip>> _shipPools = new List<SpawnPool<EnemyShip>>();
    private readonly List<SpawnPool<Ammo>> _ammoPools = new List<SpawnPool<Ammo>>();
    private readonly List<EnemySetup> _escaped = new List<EnemySetup>();

    private Camera _camera;
    private EnemyContext _context;
    private LevelPlanner _planner;
    private Action<EnemyShip> _created;
    private Coroutine _spawning;
    private Wave _wave;
    private List<PlannedSpawn> _plan;
    private int _released;

    public event Action<EnemyShip> EnemyDied;
    public event Action WaveCleared;
    public event Action<string, float> BossAnnounced;

    private void Awake()
    {
        _camera = Camera.main;
        _created = OnEnemyCreated;
        _context = new EnemyContext(_pools, _explosionPrefab, _deadBodyPrefab,
            new ShotAudio(_hitAudioSource, _camera), _targetLayers, _camera);
        _planner = new LevelPlanner(_enemies, _weapons, _bosses, _waves);
    }

    public void StartLevel(string locationId, int level)
    {
        _shipPools.Clear();
        _ammoPools.Clear();
        ClearField();

        List<PlannedSpawn> plan = _planner.Plan(locationId, level);

        foreach (PlannedSpawn spawn in plan)
            CollectPools(spawn.Setup);

        _plan = plan;
        _released = 0;
        _wave = new Wave(plan.Count);
        _spawning = StartCoroutine(Spawn(plan));
    }

    public int CountPlanned(string locationId, int levels)
    {
        int count = 0;

        for (int level = 1; level <= levels; level++)
            count += _planner.Plan(locationId, level).Count;

        return count;
    }

    public void FinishWave()
    {
        EndWave();

        foreach (SpawnPool<Ammo> pool in _ammoPools)
            pool.ForEachActive(ammo => ammo.Disarm());
    }

    public void DefeatAll()
    {
        if (_wave == null)
            return;

        StopSpawn();

        _wave = null;

        foreach (SpawnPool<EnemyShip> pool in _shipPools)
            pool.ForEachActive(ship => ship.Die());

        foreach (EnemySetup setup in _escaped)
            Release(setup).Die();

        for (int i = _released; i < _plan.Count; i++)
            Release(_plan[i].Setup).Die();

        _escaped.Clear();
        WaveCleared?.Invoke();
    }

    public void StopSpawn()
    {
        if (_spawning == null)
            return;

        StopCoroutine(_spawning);
        _spawning = null;
    }

    public void ClearField()
    {
        EndWave();
        _pools.ReleaseAll();
    }

    private void CollectPools(EnemySetup setup)
    {
        Collect(_shipPools, _pools.GetPool(setup.Prefab, _created));
        CollectAmmo(setup.Armament);

        foreach (BossPhase phase in setup.Phases)
            CollectAmmo(phase.Armament);
    }

    private void CollectAmmo(EnemyArmament armament)
    {
        if (armament.Weapon != null && armament.Weapon.Projectile != null)
            Collect(_ammoPools, _pools.GetPool(armament.Weapon.Projectile));
    }

    private void Collect<T>(List<SpawnPool<T>> pools, SpawnPool<T> pool)
        where T : MonoBehaviour, IPoolable
    {
        if (pools.Contains(pool) == false)
            pools.Add(pool);
    }

    private void EndWave()
    {
        StopSpawn();

        _wave = null;
        _escaped.Clear();

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

        if (ship.IsDead == false)
            _escaped.Add(ship.Setup);

        _wave.CountRetired();

        if (_wave.IsCleared)
            WaveCleared?.Invoke();
    }

    private IEnumerator Spawn(List<PlannedSpawn> plan)
    {
        for (int i = 0; i < plan.Count; i++)
        {
            PlannedSpawn spawn = plan[i];

            while (spawn.WaitsForClearField && _wave.RetiredCount < i)
                yield return null;

            if (spawn.HasAnnouncement)
            {
                BossAnnounced?.Invoke(spawn.Announcement, _announcementDuration);

                for (float left = _announcementDuration; left > 0f; left -= Time.deltaTime)
                    yield return null;
            }

            Release(spawn.Setup);
            _released = i + 1;

            for (float left = spawn.Interval; left > 0f; left -= Time.deltaTime)
                yield return null;
        }

        _spawning = null;
    }

    public void Spawn(EnemySetup setup)
    {
        CollectPools(setup);
        Release(setup);
    }

    private EnemyShip Release(EnemySetup setup)
    {
        SpawnPool<EnemyShip> pool = _pools.GetPool(setup.Prefab, _created);
        EnemyShip ship = pool.Get(
            setup.Movement.GetSpawnPoint(_camera, _spawnMargin, _minSpawnPositionY, _maxSpawnPositionY));

        ship.Launch(setup);

        return ship;
    }
}
