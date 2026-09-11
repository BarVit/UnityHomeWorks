using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyShip _prefab;
    [SerializeField] private float _spawnDelay;

    private ObjectPoolEnemy _pool;
    private Coroutine _spawner;
    private float _spawnPositionX = 9f;
    private float _spawnMinPositionY = -3f;
    private float _spawnMaxPositionY = 3f;

    private void Awake()
    {
        _pool = new(_prefab);
    }
    private void Start()
    {
        _spawner = StartCoroutine(Spawn());
    }

    public ObjectPoolEnemy GetPool()
    {
        return _pool;
    }

    private IEnumerator Spawn()
    {
        WaitForSeconds shootDelay = new(_spawnDelay);

        while (enabled)
        {
            EnemyShip ship = _pool.Get();

            ship.transform.position = new Vector3(_spawnPositionX, Random.Range(_spawnMinPositionY, _spawnMaxPositionY), 0);
            
            if (ship.TryGetComponent(out EnemyMover enemyMover))
                enemyMover.Fly();

            if (ship.TryGetComponent(out EnemyShooter enemyShooter))
                enemyShooter.StartShoot();

            yield return shootDelay;
        }
    }
}