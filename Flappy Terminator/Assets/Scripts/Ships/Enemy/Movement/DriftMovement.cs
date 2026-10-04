using UnityEngine;

public class DriftMovement : EnemyMovement
{
    private readonly DriftSway _sway;

    public DriftMovement(Rigidbody2D body, Camera camera, DriftSway sway) : base(body, camera)
    {
        _sway = sway;
    }

    public override void Tick(float deltaTime)
    {
        Move(Forward * Speed + _sway.Next(deltaTime));
    }

    protected override void OnBegin()
    {
        _sway.Reset();
        base.OnBegin();
    }
}
