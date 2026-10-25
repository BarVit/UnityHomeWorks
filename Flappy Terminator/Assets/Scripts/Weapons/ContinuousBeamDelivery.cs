using System.Collections.Generic;
using UnityEngine;

public class ContinuousBeamDelivery : BeamDelivery
{
    private static readonly Color CoolColor = new Color(0.25f, 1f, 0.35f);
    private static readonly Color WarmColor = new Color(1f, 0.92f, 0.2f);
    private static readonly Color HotColor = new Color(1f, 0.2f, 0.12f);

    private const float ContinuousGapShare = 5f;
    private const float FiringGapShare = 1.5f;

    private readonly Dictionary<IDamageable, float> _owed = new Dictionary<IDamageable, float>();

    private float _lastFireTime = float.NegativeInfinity;
    private float _strikeTime;
    private float _sinceFire = float.PositiveInfinity;

    public ContinuousBeamDelivery(WeaponRow row, WeaponContext context) : base(row, context)
    {
    }

    public override Transform LockTarget => Target;

    protected override bool IsHoldingHeat => Row.Owner == WeaponOwner.Player && IsOverheated == false
        && _sinceFire <= Row.Rate * FiringGapShare;

    protected override void OnFire()
    {
        float elapsed = Time.time - _lastFireTime;

        _strikeTime = elapsed <= Row.Rate * ContinuousGapShare ? elapsed : Row.Rate;
        _lastFireTime = Time.time;
        _sinceFire = 0f;
        ForgetTarget();
        ForgetHits();

        for (int barrel = 0; barrel < Barrels; barrel++)
            Shoot(barrel);

        HoldLoop();
    }

    protected override void OnTick(float deltaTime)
    {
        base.OnTick(deltaTime);
        _sinceFire += deltaTime;
    }

    protected override int GetStrikeDamage(IDamageable damageable, int barrel, out bool isCrit)
    {
        _owed.TryGetValue(damageable, out float owed);

        owed += RollDamage(GetDamage(barrel) * _strikeTime, barrel, out isCrit);

        int whole = Mathf.FloorToInt(owed);

        _owed[damageable] = owed - whole;

        return whole;
    }

    protected override void ShowStrikeCrit(Vector2 point, int damage)
    {
        ShowCrit(point);
    }

    protected override void Paint(BeamFlash flash)
    {
        flash.Tint(GetHeatColor());
    }

    private Color GetHeatColor()
    {
        float heated = HeatShare;

        if (heated < 0.5f)
            return Color.Lerp(CoolColor, WarmColor, heated * 2f);

        return Color.Lerp(WarmColor, HotColor, (heated - 0.5f) * 2f);
    }
}
