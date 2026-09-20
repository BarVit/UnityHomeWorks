using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMover : MonoBehaviour
{
    [SerializeField] private float _tapForce;
    [SerializeField] private float _maxVerticalSpeed;
    [SerializeField] private float _rotationSpeed;
    [SerializeField] private float _minRotationZ;
    [SerializeField] private float _maxRotationZ;
    [SerializeField] private Vector2 _startPosition;
    [SerializeField] private float _softStartDuration = 0.5f;
    [SerializeField] private float _aimSpeed = 720f;

    private Rigidbody2D _rigidbody2D;
    private Quaternion _minRotation;
    private Quaternion _maxRotation;
    private float _verticalSpeed;
    private float _softStartLeft;
    private float _holdLeft;
    private Transform _holdTarget;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _maxRotation = Quaternion.Euler(0, 0, _maxRotationZ);
        _minRotation = Quaternion.Euler(0, 0, _minRotationZ);
    }

    private void Update()
    {
        if (_softStartLeft > 0f)
        {
            _softStartLeft -= Time.deltaTime;
            _rigidbody2D.linearVelocity = Vector2.zero;
            transform.rotation = Quaternion.identity;

            return;
        }

        if (_holdLeft > 0f)
        {
            _holdLeft -= Time.deltaTime;
            _rigidbody2D.linearVelocity = Vector2.zero;
            AimAtTarget();

            return;
        }

        transform.rotation = Quaternion.Lerp(transform.rotation, _minRotation, _rotationSpeed * Time.deltaTime);
        _verticalSpeed = Mathf.Clamp(_rigidbody2D.linearVelocity.y, -_maxVerticalSpeed, _maxVerticalSpeed);
        _rigidbody2D.linearVelocity = new Vector2(0, _verticalSpeed);
    }

    public bool IsHolding => _holdLeft > 0f;

    public void Hold(float duration, Transform target)
    {
        _holdLeft = duration;
        _holdTarget = target;
    }

    public void Jump()
    {
        _softStartLeft = 0f;
        _holdLeft = 0f;
        _rigidbody2D.linearVelocity = new Vector2(0, _tapForce);
        transform.rotation = _maxRotation;
    }

    public void Stop()
    {
        _holdLeft = 0f;
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
        _holdLeft = 0f;
        _softStartLeft = _softStartDuration;
        StopMotion();
        _rigidbody2D.rotation = 0f;
        _rigidbody2D.position = _startPosition;
        transform.SetPositionAndRotation(_startPosition, Quaternion.identity);
    }

    private void AimAtTarget()
    {
        if (_holdTarget == null || _holdTarget.gameObject.activeInHierarchy == false)
            return;

        Vector2 direction = (Vector2)(_holdTarget.position - transform.position);

        if (direction.sqrMagnitude < Mathf.Epsilon)
            return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.RotateTowards(transform.rotation,
            Quaternion.Euler(0f, 0f, angle), _aimSpeed * Time.deltaTime);
    }

    private void StopMotion()
    {
        _rigidbody2D.linearVelocity = Vector2.zero;
        _rigidbody2D.angularVelocity = 0f;
    }

    public void Push()
    {
        Vector2 direction = Vector2.down;

        if (transform.position.y <= 0)
        {
            direction = Vector2.up;
            transform.rotation = _maxRotation;
        }

        _rigidbody2D.linearVelocity = direction * _maxVerticalSpeed;
    }
}
