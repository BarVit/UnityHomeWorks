using UnityEngine;

[RequireComponent(typeof(EnemyPool))]
public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private Target _target;

    private EnemyPool _enemyPool;

    private void Awake()
    {
        _enemyPool = GetComponent<EnemyPool>();
    }

    public void SpawnEnemy()
    {
        Enemy enemy = _enemyPool.GetEnemy;

        enemy.transform.position = transform.position;
        enemy.SetTarget(_target.transform);
    }
}