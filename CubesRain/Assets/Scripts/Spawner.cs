using UnityEngine;
using UnityEngine.Pool;
using System.Collections;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject _prefab;

    private int _poolCapacity = 10;
    private int _poolMaxSize = 100;

    private float _spawnRate = 0.5f;

    private ObjectPool<Cube> _pool;

    private void Awake()
    {
        Cube cube = _prefab.GetComponent<Cube>();

        _pool = new ObjectPool<Cube>(
            createFunc: () => Instantiate(cube),
            actionOnGet: (obj) => InitCube(obj),
            actionOnRelease: (obj) => obj.gameObject.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj),
            collectionCheck: true,
            defaultCapacity: _poolCapacity,
            maxSize: _poolMaxSize);
    }

    private void OnEnable()
    {
        Cube.Collided += HandleCollision;
    }

    private void OnDisable()
    {
        Cube.Collided -= HandleCollision;
    }

    private void Start()
    {
        StartCoroutine(Spawn());
    }

    private IEnumerator Spawn()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(_spawnRate);

        while (true)
        {
            _pool.Get();
            yield return waitForSeconds;
        }
    }

    private void InitCube(Cube cube)
    {
        cube.Init();
        cube.transform.position = GetSpawnPosition();
        cube.gameObject.SetActive(true);
    }

    private void HandleCollision(Cube cube)
    {
        StartCoroutine(RealeseCube(cube));
    }

    private IEnumerator RealeseCube(Cube cube)
    {
        float minDestroyTime = 2f;
        float maxDestroyTime = 5f;

        WaitForSeconds waitForSeconds = new WaitForSeconds(Random.Range(minDestroyTime, maxDestroyTime));

        yield return waitForSeconds;
        _pool.Release(cube);
    }

    private Vector3 GetSpawnPosition()
    {
        int topCorner = 9;
        int bottomCorner = -9;
        int leftCorner = -9;
        int rightCorner = 9;
        int minHegith = 20;
        int maxHeight = 25;

        return new Vector3(Random.Range(leftCorner, rightCorner),
            Random.Range(minHegith, maxHeight), Random.Range(bottomCorner, topCorner));
    }
}