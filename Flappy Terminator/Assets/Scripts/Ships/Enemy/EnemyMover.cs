using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMover : MonoBehaviour
{
    [SerializeField] private float _minSpeed = 1f;
    [SerializeField] private float _maxSpeed = 3f;
    [SerializeField] private MovementSettings _movementSettings = new MovementSettings();

    private Rigidbody2D _rigidbody2D;
    private ForwardMovement _forward;
    private DriftMovement _drift;
    private HoverMovement _hover;
    private EnemyMovement _movement;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _forward = new ForwardMovement(_rigidbody2D, _movementSettings);
        _drift = new DriftMovement(_rigidbody2D, _movementSettings);
        _hover = new HoverMovement(_rigidbody2D, _movementSettings);
    }

    private void Update()
    {
        if (_movement == null)
            return;

        _movement.Tick(Time.deltaTime);
    }

    public void Fly(MovementKind kind)
    {
        _movement = GetMovement(kind);
        _movement.Begin(Random.Range(_minSpeed, _maxSpeed));
    }

    public void Stop()
    {
        if (_movement == null)
        {
            _rigidbody2D.linearVelocity = Vector2.zero;

            return;
        }

        _movement.Stop();
        _movement = null;
    }

    private EnemyMovement GetMovement(MovementKind kind)
    {
        switch (kind)
        {
            case MovementKind.Drift:
                return _drift;

            case MovementKind.Hover:
                return _hover;

            default:
                return _forward;
        }
    }
}
