using UnityEngine;

public class DriftRetreatMovement : EnemyMovement
{
    private readonly DriftRetreatProfile _profile;
    private readonly DriftSway _sway;

    private Stage _stage;
    private float _retreatEndX;

    public DriftRetreatMovement(Rigidbody2D body, Camera camera, DriftRetreatProfile profile, DriftSway sway)
        : base(body, camera)
    {
        _profile = profile;
        _sway = sway;
    }

    private enum Stage
    {
        Advance,
        Retreat,
        Leave
    }

    public override void Tick(float deltaTime)
    {
        UpdateStage();

        float direction = _stage == Stage.Retreat ? -1f : 1f;

        Move(Forward * (Speed * direction) + _sway.Next(deltaTime));
    }

    protected override void OnBegin()
    {
        _stage = Stage.Advance;
        _sway.Reset();
        base.OnBegin();
    }

    private void UpdateStage()
    {
        if (_stage == Stage.Advance && HasPassed(_profile.AdvanceShare))
        {
            _stage = Stage.Retreat;
            _retreatEndX = Body.position.x + _profile.RetreatShare * ScreenWidth;
        }
        else if (_stage == Stage.Retreat && Body.position.x >= _retreatEndX)
        {
            _stage = Stage.Leave;
        }
    }
}
