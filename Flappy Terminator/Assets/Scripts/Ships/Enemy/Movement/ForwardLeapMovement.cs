using UnityEngine;

public class ForwardLeapMovement : EnemyMovement
{
    private readonly ForwardLeapProfile _profile;

    private float _leapStartShare;
    private float _leapEndX;
    private bool _isLeaping;
    private bool _isLeapSpent;

    public ForwardLeapMovement(Rigidbody2D body, Camera camera, ForwardLeapProfile profile) : base(body, camera)
    {
        _profile = profile;
    }

    public override void Tick(float deltaTime)
    {
        if (_isLeapSpent)
            return;

        if (_isLeaping)
        {
            if (Body.position.x > _leapEndX)
                return;

            _isLeaping = false;
            _isLeapSpent = true;
            MoveForward();

            return;
        }

        if (HasPassed(_leapStartShare) == false)
            return;

        _isLeaping = true;
        _leapEndX = Body.position.x - _profile.LeapShare * ScreenWidth;
        Move(Forward * _profile.LeapSpeed);
    }

    protected override void OnBegin()
    {
        _leapStartShare = _profile.GetLeapStartShare();
        _isLeaping = false;
        _isLeapSpent = false;
        base.OnBegin();
    }
}
