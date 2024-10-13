using System.Collections.Generic;
using UnityEngine;

public class SpawnPointGenerator : MonoBehaviour
{
    [SerializeField] private SpawnPoint<Enemy> _prefab;
    [SerializeField] private List<Enemy> _enemies;

    private List<SpawnPoint<Enemy>> _spawnPoints = new();

    private int _spawnPointsAmount = 5;

    private void Awake()
    {
        string spawnPointName = "Spawn point";

        for (int i = 0; i < _spawnPointsAmount; i++)
        {
            SpawnPoint<> spawnPoint = Instantiate(_prefab);
            
            spawnPoint.gameObject.name = spawnPointName + " " + spawnPoint.GetType();
            spawnPoint.transform.position = GetRandomPosition();
            _spawnPoints.Add(spawnPoint);
        }
    }

    public Vector3 GetRandomWaypoint()
    {
        return _spawnPoints[Random.Range(0, _spawnPoints.Count)].transform.position;
    }

    private Vector3 GetRandomPosition()
    {
        Vector3 position = Vector3.zero;

        float minX = -20f;
        float maxX = 20f;
        float minZ = -20f;
        float maxZ = 20f;
        float posY = 0.5f;
        float minDistance = 5;
        float distance = 0;

        int counter = 0;

        while (distance <= minDistance && counter++ < 50)
        {
            position = new Vector3(Random.Range(minX, maxX), posY, Random.Range(minZ, maxZ));

            for (int i = 0; i < _spawnPoints.Count; i++)
            {
                distance = Mathf.Max(Vector3.Distance(position, _spawnPoints[i].transform.position), minDistance);
            }
        }

        return position;
    }

    private Enemy GetNextEnemyClass(int index)
    {
        List<Enemy> enemies = new()
        {
            new Human(),
            new Orc(),
            new Elf(),
            new Dwarf(),
            new Goblin()
        };

        if (index < 0 || index > enemies.Count - 1)
            return enemies[0];
        else
            return enemies[index];
    }
}