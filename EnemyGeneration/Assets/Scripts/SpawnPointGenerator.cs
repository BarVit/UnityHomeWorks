using System.Collections.Generic;
using UnityEngine;

public class SpawnPointGenerator : MonoBehaviour
{
    [SerializeField] private SpawnPoint _prefab;

    private List<SpawnPoint> _spawnPoints = new();

    private int _spawnPointsAmount = 5;

    private void Awake()
    {
        string spawnPointName = "Spawn point";

        for (int i = 0; i < _spawnPointsAmount; i++)
        {
            SpawnPoint spawnPoint = Instantiate(_prefab);

            spawnPoint.gameObject.name = spawnPointName + " " + (i + 1);
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
        float minX = -20f;
        float maxX = 20f;
        float minZ = -20f;
        float maxZ = 20f;
        float posY = 0.5f;

        return new Vector3(Random.Range(minX, maxX), posY, Random.Range(minZ, maxZ));
    }
}