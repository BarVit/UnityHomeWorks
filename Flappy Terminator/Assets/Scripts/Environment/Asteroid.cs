using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Asteroid : MonoBehaviour, IRemoveable
{
    [SerializeField] private float _minSpeed = 0.5f;
    [SerializeField] private float _maxSpeed = 3f;
    [SerializeField] private float _minRotationSpeed = -60f;
    [SerializeField] private float _maxRotationSpeed = 60f;

    private Rigidbody2D _rigidbody2D;
    private float _minTopSpawnBoundY = 4f;
    private float _maxTopSpawnBoundY = 5.3f;
    private float _minBottomSpawnBoundY = -4f;
    private float _maxBottomSpawnBoundY = -5.3f;
    private float _spawnPositionX = 9.5f;
    private float _speed;
    private float _rotationSpeed;

    public event Action<Asteroid> Bumped;

    [field: SerializeField] public int Damage { get; private set; }

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        _rigidbody2D.rotation += _rotationSpeed * Time.deltaTime;
        _rigidbody2D.velocity = new Vector2(-_speed, 0);
    }

    public void Remove()
    {
        Bumped?.Invoke(this);
    }

    private void Init()
    {
        _speed = UnityEngine.Random.Range(_minSpeed, _maxSpeed);
        _rotationSpeed = UnityEngine.Random.Range(_minRotationSpeed, _maxRotationSpeed); ;
        transform.position = UnityEngine.Random.Range(0, 2) == 1 ?
            new Vector2(_spawnPositionX, UnityEngine.Random.Range(_minTopSpawnBoundY, _maxTopSpawnBoundY)) :
            new Vector2(_spawnPositionX, UnityEngine.Random.Range(_minBottomSpawnBoundY, _maxBottomSpawnBoundY));
    }
}