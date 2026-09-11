using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AsteroidSpawner : MonoBehaviour
{
    [SerializeField] private List<Asteroid> _prefabs;
    [SerializeField] private float _spawnDelay;

    private List<ObjectPoolAsteroid> _pools;
    private Coroutine _spawner;

    private void Start()
    {
        _pools = new();

        for (int i = 0; i < _prefabs.Count; i++)
        {
            ObjectPoolAsteroid pool = new(_prefabs[i]);
            _pools.Add(pool);
        }

        _spawner = StartCoroutine(Spawn());
    }

    private IEnumerator Spawn()
    {
        WaitForSeconds shootDelay = new(_spawnDelay);

        while (enabled)
        {
            Asteroid asteroid;
            ObjectPoolAsteroid pool;
            float posX = 10f;
            float minPosBottomY = -5.3f;
            float maxPosBottomY = -4f;
            float minPosTopY = 4f;
            float maxPosTopY = 5.3f;

            pool = _pools[Random.Range(0, _prefabs.Count)];
            asteroid = pool.Get();
            asteroid.transform.position = new Vector3(posX, Random.Range(minPosBottomY, maxPosBottomY), 0);
            asteroid = pool.Get();
            asteroid.transform.position = new Vector3(posX, Random.Range(minPosTopY, maxPosTopY), 0);

            yield return shootDelay;
        }
    }
}