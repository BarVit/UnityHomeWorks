using UnityEngine;

public class HoverMovement : EnemyMovement
{
    private float _holdPositionX;
    private float _holdLeft;
    private bool _isHolding;
    private bool _isHoldSpent;

    public HoverMovement(Rigidbody2D body, MovementSettings settings) : base(body, settings)
    {
    }

    public override void Tick(float deltaTime)
    {
        if (_isHoldSpent)
            return;

        if (_isHolding)
        {
            _holdLeft -= deltaTime;

            if (_holdLeft > 0f)
                return;

            _isHolding = false;
            _isHoldSpent = true;
            MoveForward();

            return;
        }

        if (Body.position.x > _holdPositionX)
            return;

        _isHolding = true;
        Stop();
    }

    protected override void OnBegin()
    {
        _holdPositionX = Random.Range(Settings.MinHoldPositionX, Settings.MaxHoldPositionX);
        _holdLeft = Settings.HoldDuration;
        _isHolding = false;
        _isHoldSpent = false;
        base.OnBegin();
    }
}
