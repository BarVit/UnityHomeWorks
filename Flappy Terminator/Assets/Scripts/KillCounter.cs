using System;
using UnityEngine;

[RequireComponent(typeof(EnemySpawner))]
public class KillCounter : MonoBehaviour
{
    private EnemySpawner _enemySpawner;

    public int Kills { get; private set; }

    public event Action<int> Changed;

    private void Awake()
    {
        _enemySpawner = GetComponent<EnemySpawner>();
    }

    private void OnEnable()
    {
        _enemySpawner.EnemyDied += OnEnemyDied;
    }

    private void OnDisable()
    {
        _enemySpawner.EnemyDied -= OnEnemyDied;
    }

    private void OnEnemyDied(EnemyShip ship)
    {
        Kills++;
        Changed?.Invoke(Kills);
    }
}
