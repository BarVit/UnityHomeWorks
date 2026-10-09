using UnityEngine;

public class UpDownMovement : EnemyMovement
{
    private readonly UpDownProfile _profile;
    private readonly SpriteRenderer _sprite;

    private float _patrolLeft;
    private float _direction;
    private bool _isPatrolling;
    private bool _isPatrolSpent;

    public UpDownMovement(Rigidbody2D body, Camera camera, UpDownProfile profile) : base(body, camera)
    {
        _profile = profile;
        _sprite = body.GetComponentInChildren<SpriteRenderer>(true);
    }

    public override void Tick(float deltaTime)
    {
        if (_isPatrolSpent)
            return;

        if (_isPatrolling == false)
        {
            if (HasEntered())
                StartPatrol();

            return;
        }

        if (_profile.IsEndless == false)
        {
            _patrolLeft -= deltaTime;

            if (_patrolLeft <= 0f)
            {
                _isPatrolling = false;
                _isPatrolSpent = true;
                MoveForward();

                return;
            }
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

    private bool HasEntered()
    {
        if (_profile.KeepsBodyGap == false || _sprite == null)
            return HasPassed(_profile.EntryShare);

        Bounds bounds = _sprite.bounds;

        return bounds.max.x <= GetX(0f) - bounds.size.x * _profile.BodyGapShare;
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
        if (_profile.KeepsBodyGap && _sprite != null)
        {
            TurnByBody(_sprite.bounds);

            return;
        }

        float y = Body.position.y;

        if (y >= Top - _profile.EdgeMargin)
            _direction = -1f;
        else if (y <= Bottom + _profile.EdgeMargin)
            _direction = 1f;
    }

    private void TurnByBody(Bounds bounds)
    {
        float gap = bounds.size.y * _profile.BodyGapShare;

        if (bounds.max.y >= Top - gap)
            _direction = -1f;
        else if (bounds.min.y <= Bottom + gap)
            _direction = 1f;
    }
}
