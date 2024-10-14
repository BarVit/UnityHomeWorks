using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<SpawnPoint> _spawnPoints;

    private EnemyPool _enemyPool;
    private float _spawnRate = 2f;

    public void Awake()
    {
        _enemyPool = GetComponent<EnemyPool>();
        StartCoroutine(Spawn());
    }

    private IEnumerator Spawn()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(_spawnRate);

        while (true)
        {
            SpawnPoint spawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Count)];

            Enemy enemy = _enemyPool.GetEnemy(spawnPoint.Enemy);

            enemy.transform.position = spawnPoint.transform.position;
            enemy.SetTarget(spawnPoint.Target.transform);
            yield return waitForSeconds;
        }
    }
}