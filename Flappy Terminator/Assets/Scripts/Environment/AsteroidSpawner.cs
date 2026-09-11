using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    [SerializeField] private List<Asteroid> _prefabs;
    [SerializeField] private float _spawnDelay;
    [SerializeField] private float _spawnPositionX = 10f;
    [SerializeField] private float _minBottomPositionY = -5.3f;
    [SerializeField] private float _maxBottomPositionY = -4f;
    [SerializeField] private float _minTopPositionY = 4f;
    [SerializeField] private float _maxTopPositionY = 5.3f;

    private List<SpawnPool<Asteroid>> _pools;
    private Coroutine _spawning;

    private void Awake()
    {
        _pools = new List<SpawnPool<Asteroid>>();

        foreach (Asteroid prefab in _prefabs)
            _pools.Add(new SpawnPool<Asteroid>(prefab));
    }

    private void Start()
    {
        _spawning = StartCoroutine(Spawn());
    }

    private IEnumerator Spawn()
    {
        WaitForSeconds spawnDelay = new(_spawnDelay);

        while (enabled)
        {
            SpawnPool<Asteroid> pool = _pools[Random.Range(0, _pools.Count)];

            Spawn(pool, Random.Range(_minBottomPositionY, _maxBottomPositionY));
            Spawn(pool, Random.Range(_minTopPositionY, _maxTopPositionY));

            yield return spawnDelay;
        }
    }

    private void Spawn(SpawnPool<Asteroid> pool, float positionY)
    {
        Asteroid asteroid = pool.Get();

        asteroid.transform.position = new Vector3(_spawnPositionX, positionY, 0);
    }
}
