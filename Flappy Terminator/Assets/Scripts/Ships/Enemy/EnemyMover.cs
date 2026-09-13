using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMover : MonoBehaviour
{
    [SerializeField] private float _minSpeed = 1f;
    [SerializeField] private float _maxSpeed = 3f;

    private Rigidbody2D _rigidbody2D;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    public void Fly()
    {
        float speed = Random.Range(_minSpeed, _maxSpeed);

        _rigidbody2D.linearVelocity = transform.right * -speed;
    }

    public void Stop()
    {
        _rigidbody2D.linearVelocity = Vector2.zero;
    }
}
