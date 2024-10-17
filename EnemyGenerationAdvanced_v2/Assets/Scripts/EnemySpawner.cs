using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<SpawnPoint> _spawnPoints;

    private float _spawnRate = 2f;

    private void Awake()
    {
        StartCoroutine(Spawn());
    }

    private IEnumerator Spawn()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(_spawnRate);

        while (true)
        {
            SpawnPoint spawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Count)];

            spawnPoint.SpawnEnemy();
            yield return waitForSeconds;
        }
    }
}