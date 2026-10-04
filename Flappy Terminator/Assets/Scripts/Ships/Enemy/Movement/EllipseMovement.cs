using UnityEngine;

public class EllipseMovement : EnemyMovement
{
    private readonly EllipseProfile _profile;
    private readonly SpriteRenderer _sprite;

    private float _radius;
    private float _angle;
    private float _turn;
    private bool _isLooping;
    private bool _isGone;

    public EllipseMovement(Rigidbody2D body, Camera camera, EllipseProfile profile) : base(body, camera)
    {
        _profile = profile;
        _sprite = body.GetComponentInChildren<SpriteRenderer>(true);
    }

    public override void Tick(float deltaTime)
    {
        if (_isGone)
            return;

        if (_isLooping == false)
        {
            _isLooping = HasPassed(_profile.LoopStartShare);

            return;
        }

        if (IsBehindLeftEdge())
        {
            _isGone = true;
            Move(Vector2.left * Speed);

            return;
        }

        _angle += _turn * Speed / _radius * deltaTime;
        Steer();
    }

    protected override void OnBegin()
    {
        _radius = Mathf.Max(0.01f, _profile.GetRadiusShare() * ScreenHeight);
        _angle = Mathf.PI;
        _turn = Body.position.y > CenterY ? 1f : -1f;
        _isLooping = false;
        _isGone = false;
        Steer();
    }

    private void Steer()
    {
        Vector2 heading = new Vector2(Mathf.Cos(_angle), Mathf.Sin(_angle));
        float speed = heading.x < 0f ? Speed : Speed * _profile.BackwardSpeedShare;

        Move(heading * speed);
    }

    private bool IsBehindLeftEdge()
    {
        float right = _sprite == null ? Body.position.x : _sprite.bounds.max.x;

        return right < LeftEdge;
    }
}
