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

    private Vector2 _startPosition;
    private Rigidbody2D _rigidbody2D;
    private Quaternion _minRotation;
    private Quaternion _maxRotation;
    private float _verticalSpeed;

    private void Start()
    {
        _startPosition = transform.position;
        _rigidbody2D = GetComponent<Rigidbody2D>();

        _maxRotation = Quaternion.Euler(0, 0, _maxRotationZ);
        _minRotation = Quaternion.Euler(0, 0, _minRotationZ);

        ReturnToStart();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            _rigidbody2D.linearVelocity = new Vector2(0, _tapForce);
            transform.rotation = _maxRotation;
        }

        transform.rotation = Quaternion.Lerp(transform.rotation, _minRotation, _rotationSpeed * Time.deltaTime);
        _verticalSpeed = Mathf.Clamp(_rigidbody2D.linearVelocity.y, -_maxVerticalSpeed, _maxVerticalSpeed);
        _rigidbody2D.linearVelocity = new Vector2(0, _verticalSpeed);
    }

    public void ReturnToStart()
    {
        transform.position = _startPosition;
        transform.rotation = Quaternion.identity;
        _rigidbody2D.linearVelocity = Vector2.zero;
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