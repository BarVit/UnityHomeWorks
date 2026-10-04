using UnityEngine;

public class HoverMovement : EnemyMovement
{
    private readonly HoverProfile _profile;

    private float _firstStopShare;
    private int _nextStop;
    private float _holdLeft;
    private bool _isHolding;
    private bool _hasStopped;

    public HoverMovement(Rigidbody2D body, Camera camera, HoverProfile profile) : base(body, camera)
    {
        _profile = profile;
    }

    public override void Tick(float deltaTime)
    {
        if (_isHolding)
        {
            Hold(deltaTime);

            return;
        }

        if (_nextStop >= _profile.StopCount || HasPassed(GetStopShare(_nextStop)) == false)
            return;

        bool isLast = _nextStop == _profile.StopCount - 1;

        _nextStop++;

        if (ShouldStop(isLast) == false)
            return;

        _hasStopped = true;
        _isHolding = true;
        _holdLeft = _profile.GetHold();
        Stop();
    }

    protected override void OnBegin()
    {
        _firstStopShare = _profile.GetFirstStopShare();
        _nextStop = 0;
        _isHolding = false;
        _hasStopped = false;
        base.OnBegin();
    }

    private void Hold(float deltaTime)
    {
        _holdLeft -= deltaTime;

        if (_holdLeft > 0f)
            return;

        _isHolding = false;
        MoveForward();
    }

    private bool ShouldStop(bool isLast)
    {
        return (isLast && _hasStopped == false) || Random.value < _profile.StopChance;
    }

    private float GetStopShare(int stop)
    {
        return _firstStopShare + stop * _profile.StopStep;
    }
}
