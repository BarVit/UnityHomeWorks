using UnityEngine;

public abstract class EnemyMovement
{
    private readonly Rigidbody2D _body;
    private readonly Camera _camera;

    private float _speed;

    protected EnemyMovement(Rigidbody2D body, Camera camera)
    {
        _body = body;
        _camera = camera;
    }

    protected Rigidbody2D Body => _body;

    protected float Speed => _speed;

    protected Vector2 Forward => -(Vector2)_body.transform.right;

    protected float ScreenWidth => ScreenArea.GetRightEdge(_camera) - ScreenArea.GetLeftEdge(_camera);

    protected float ScreenHeight => _camera.orthographicSize * 2f;

    protected float LeftEdge => ScreenArea.GetLeftEdge(_camera);

    protected float Top => _camera.transform.position.y + _camera.orthographicSize;

    protected float Bottom => _camera.transform.position.y - _camera.orthographicSize;

    protected float CenterY => _camera.transform.position.y;

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

    protected void Move(Vector2 velocity)
    {
        _body.linearVelocity = velocity;
    }

    protected void MoveForward()
    {
        Move(Forward * _speed);
    }

    protected float GetX(float screenShare)
    {
        return ScreenArea.GetRightEdge(_camera) - screenShare * ScreenWidth;
    }

    protected bool HasPassed(float screenShare)
    {
        return _body.position.x <= GetX(screenShare);
    }

    protected virtual void OnBegin()
    {
        MoveForward();
    }
}
