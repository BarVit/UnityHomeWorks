using UnityEngine;

public class ContinuousBeamDelivery : BeamDelivery
{
    private const float MaxHeat = 100f;

    private float _heat;
    private bool _isOverheated;

    public ContinuousBeamDelivery(WeaponRow row, WeaponContext context) : base(row, context)
    {
    }

    public override bool IsReady => base.IsReady && _isOverheated == false;

    public override float Readiness => 1f - Mathf.Clamp01(_heat / MaxHeat);

    public bool IsOverheated => _isOverheated;

    public float Heat => _heat;

    protected override void OnFire()
    {
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
        if (_heat <= 0f)
            return;

        _heat -= Row.CooldownRate * deltaTime;

        if (_heat > 0f)
            return;

        _heat = 0f;
        _isOverheated = false;
    }
}
