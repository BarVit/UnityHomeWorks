using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(EnemyPool))]
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

            Enemy enemy = spawnPoint.Enemy;

            GameObject enemyPrefab = _enemyPool.GetEnemy;

            enemyPrefab.AddComponent(enemy);
            enemyPrefab.transform.position = spawnPoint.transform.position;
            enemyPrefab.SetTarget(spawnPoint.Target.transform);
            yield return waitForSeconds;
        }
    }
}