using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMover : MonoBehaviour
{
    [SerializeField] private float _minSpeed = 1f;
    [SerializeField] private float _maxSpeed = 3f;

    private Rigidbody2D _rigidbody2D;
    private float _speed;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _speed = Random.Range(_minSpeed, _maxSpeed);
    }

    public void Fly()
    {
        _rigidbody2D.velocity = transform.right * -_speed;
    }

    public void Reset()
    {
        _rigidbody2D.velocity = Vector2.zero;
    }
}