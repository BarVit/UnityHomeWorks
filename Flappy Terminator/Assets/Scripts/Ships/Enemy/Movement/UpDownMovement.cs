using UnityEngine;

public class UpDownMovement : EnemyMovement
{
    private readonly UpDownProfile _profile;

    private float _patrolLeft;
    private float _direction;
    private bool _isPatrolling;
    private bool _isPatrolSpent;

    public UpDownMovement(Rigidbody2D body, Camera camera, UpDownProfile profile) : base(body, camera)
    {
        _profile = profile;
    }

    public override void Tick(float deltaTime)
    {
        if (_isPatrolSpent)
            return;

        if (_isPatrolling == false)
        {
            if (HasPassed(_profile.EntryShare))
                StartPatrol();

            return;
        }

        _patrolLeft -= deltaTime;

        if (_patrolLeft <= 0f)
        {
            _isPatrolling = false;
            _isPatrolSpent = true;
            MoveForward();

            return;
        }

        Turn();
        Move(Vector2.up * (_direction * Speed));
    }

    protected override void OnBegin()
    {
        _isPatrolling = false;
        _isPatrolSpent = false;
        base.OnBegin();
    }

    private void StartPatrol()
    {
        _isPatrolling = true;
        _patrolLeft = _profile.Duration;
        _direction = Random.value < 0.5f ? 1f : -1f;
        Move(Vector2.up * (_direction * Speed));
    }

    private void Turn()
    {
        float y = Body.position.y;

        if (y >= Top - _profile.EdgeMargin)
            _direction = -1f;
        else if (y <= Bottom + _profile.EdgeMargin)
            _direction = 1f;
    }
}
