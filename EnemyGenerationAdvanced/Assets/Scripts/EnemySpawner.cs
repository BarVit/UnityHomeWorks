using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public void Spawn(var t)
    {
        EnemyPool<t> enemyPool = new();

        return enemyPool.NextEnemy;
    }
}