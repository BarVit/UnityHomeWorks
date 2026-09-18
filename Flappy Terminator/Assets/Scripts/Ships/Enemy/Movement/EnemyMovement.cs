using UnityEngine;

public abstract class EnemyMovement
{
    private readonly Rigidbody2D _body;
    private readonly MovementSettings _settings;

    private float _speed;

    protected EnemyMovement(Rigidbody2D body, MovementSettings settings)
    {
        _body = body;
        _settings = settings;
    }

    protected Rigidbody2D Body => _body;

    protected MovementSettings Settings => _settings;

    protected float Speed => _speed;

    public void Begin(float speed)
    {
        _speed = speed;
        OnBegin();
    }

    public virtual void Tick(float deltaTime)
    {
    }

    public void Stop()
    {
        _body.linearVelocity = Vector2.zero;
    }

    protected void MoveForward()
    {
        _body.linearVelocity = _body.transform.right * -_speed;
    }

    protected virtual void OnBegin()
    {
        MoveForward();
    }
}
