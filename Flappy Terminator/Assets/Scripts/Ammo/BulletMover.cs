using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BulletMover : MonoBehaviour
{
    [SerializeField] private float _speed;

    private Rigidbody2D _rigidbody2D;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    public void Fly()
    {
        _rigidbody2D.linearVelocity = transform.right * _speed;
    }

    public void Reset()
    {
        _rigidbody2D.linearVelocity = Vector2.zero;
    }
}