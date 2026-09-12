using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMover : MonoBehaviour
{
    [SerializeField] private float _tapForce;
    [SerializeField] private float _pushForce;
    [SerializeField] private float _maxVerticalSpeed;
    [SerializeField] private float _rotationSpeed;
    [SerializeField] private float _minRotationZ;
    [SerializeField] private float _maxRotationZ;
    [SerializeField] private Vector2 _startPosition;

    private Rigidbody2D _rigidbody2D;
    private Quaternion _minRotation;
    private Quaternion _maxRotation;
    private float _verticalSpeed;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        _maxRotation = Quaternion.Euler(0, 0, _maxRotationZ);
        _minRotation = Quaternion.Euler(0, 0, _minRotationZ);

        ReturnToStart();
    }

    private void Update()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, _minRotation, _rotationSpeed * Time.deltaTime);
        _verticalSpeed = Mathf.Clamp(_rigidbody2D.linearVelocity.y, -_maxVerticalSpeed, _maxVerticalSpeed);
        _rigidbody2D.linearVelocity = new Vector2(0, _verticalSpeed);
    }

    public void Jump()
    {
        _rigidbody2D.linearVelocity = new Vector2(0, _tapForce);
        transform.rotation = _maxRotation;
    }

    public void Stop()
    {
        StopMotion();
        _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
    }

    public void Resume()
    {
        _rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        StopMotion();
    }

    public void ReturnToStart()
    {
        StopMotion();
        _rigidbody2D.rotation = 0f;
        _rigidbody2D.position = _startPosition;
        transform.SetPositionAndRotation(_startPosition, Quaternion.identity);
    }

    private void StopMotion()
    {
        _rigidbody2D.linearVelocity = Vector2.zero;
        _rigidbody2D.angularVelocity = 0f;
    }

    public void Push()
    {
        Vector2 direction;

        if(transform.position.y > 0)
        {
            direction = Vector2.down;
            
        }
        else
        {
            direction = Vector2.up;
            transform.rotation = _maxRotation;
        }

        _rigidbody2D.AddForce(direction * _pushForce, ForceMode2D.Impulse);
    }
}