using UnityEngine;
using UnityEngine.Pool;

public class Spawner : MonoBehaviour
{
    [SerializeField] GameObject _prefab;

    private int _poolCapacity = 10;
    private int _poolMaxSize = 100;

    private float _spawnRate = 0.2f;

    private ObjectPool<GameObject> _pool;

    private void Awake()
    {
        _pool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(_prefab),
            actionOnGet: (obj) => ActionOnGet(obj),
            actionOnRelease: (obj) => obj.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj),
            collectionCheck: true,
            defaultCapacity: _poolCapacity,
            maxSize: _poolMaxSize);
    }

    private void ActionOnGet(GameObject obj)
    {
        obj.transform.position = SpawnPosition();
        obj.GetComponent<Rigidbody>().velocity = Vector3.zero;
        obj.SetActive(true);
    }

    private void Start()
    {
        InvokeRepeating(nameof(GetCube), 0, _spawnRate);
    }

    private void GetCube()
    {
        _pool.Get();
    }

    private Vector3 SpawnPosition()
    {
        int topCorner = 10;
        int bottomCorner = -10;
        int leftCorner = -10;
        int rightCorner = 10;
        int minHegith = 25;
        int maxHeight = 35;

        return new Vector3(Random.Range(leftCorner, rightCorner),
            Random.Range(minHegith, maxHeight), Random.Range(bottomCorner,topCorner));
    }    
}