using UnityEngine;

public class ContinuousBeamDelivery : BeamDelivery
{
    private const float MaxHeat = 100f;
    private const float UnlockHeat = MaxHeat * 0.5f;

    private static readonly Color CoolColor = new Color(0.25f, 1f, 0.35f);
    private static readonly Color WarmColor = new Color(1f, 0.92f, 0.2f);
    private static readonly Color HotColor = new Color(1f, 0.2f, 0.12f);

    private float _heat;
    private bool _isOverheated;

    public ContinuousBeamDelivery(WeaponRow row, WeaponContext context) : base(row, context)
    {
    }

    public override bool IsReady => base.IsReady && _isOverheated == false;

    public override float Readiness => 1f - Mathf.Clamp01(_heat / MaxHeat);

    public override bool IsOverheated => _isOverheated;

    public override bool HasHeat => true;

    public float Heat => _heat;

    public override Transform LockTarget => Target;

    protected override void OnFire()
    {
        ForgetTarget();

        for (int barrel = 0; barrel < Barrels; barrel++)
            Shoot(barrel);

        _heat += Row.HeatPerShot;

        if (_heat >= MaxHeat)
        {
            _heat = MaxHeat;
            _isOverheated = true;
        }
    }

    protected override void OnTick(float deltaTime)
    {
        base.OnTick(deltaTime);

        if (_heat <= 0f)
            return;

        _heat = Mathf.Max(0f, _heat - Row.CooldownRate * deltaTime);

        if (_heat <= UnlockHeat)
            _isOverheated = false;
    }

    protected override void Paint(BeamFlash flash)
    {
        flash.Tint(GetHeatColor());
    }

    private Color GetHeatColor()
    {
        float heated = Mathf.Clamp01(_heat / MaxHeat);

        if (heated < 0.5f)
            return Color.Lerp(CoolColor, WarmColor, heated * 2f);

        return Color.Lerp(WarmColor, HotColor, (heated - 0.5f) * 2f);
    }
}
