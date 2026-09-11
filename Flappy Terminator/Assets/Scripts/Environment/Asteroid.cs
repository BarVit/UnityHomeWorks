using System;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D))]
public class Asteroid : MonoBehaviour, IPoolable, IRemoveable
{
    [SerializeField] private float _minSpeed = 0.5f;
    [SerializeField] private float _maxSpeed = 3f;
    [SerializeField] private float _minRotationSpeed = -60f;
    [SerializeField] private float _maxRotationSpeed = 60f;

    private Rigidbody2D _rigidbody2D;
    private float _speed;
    private float _rotationSpeed;

    [field: SerializeField] public int Damage { get; private set; }

    public event Action<IPoolable> Released;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        _rigidbody2D.rotation += _rotationSpeed * Time.deltaTime;
        _rigidbody2D.linearVelocity = new Vector2(-_speed, 0);
    }

    public void OnSpawn()
    {
        _speed = Random.Range(_minSpeed, _maxSpeed);
        _rotationSpeed = Random.Range(_minRotationSpeed, _maxRotationSpeed);
    }

    public void OnDespawn()
    {
        _rigidbody2D.linearVelocity = Vector2.zero;
    }

    public void Remove()
    {
        Released?.Invoke(this);
    }
}
