using System.Collections.Generic;
using UnityEngine;

public class ContinuousBeamDelivery : BeamDelivery
{
    private static readonly Color CoolColor = new Color(0.25f, 1f, 0.35f);
    private static readonly Color WarmColor = new Color(1f, 0.92f, 0.2f);
    private static readonly Color HotColor = new Color(1f, 0.2f, 0.12f);

    private readonly Dictionary<IDamageable, float> _owed = new Dictionary<IDamageable, float>();

    public ContinuousBeamDelivery(WeaponRow row, WeaponContext context) : base(row, context)
    {
    }

    public override Transform LockTarget => Target;

    protected override void OnFire()
    {
        ForgetTarget();
        ForgetHits();

        for (int barrel = 0; barrel < Barrels; barrel++)
            Shoot(barrel);

        HoldLoop();
    }

    protected override int GetStrikeDamage(IDamageable damageable)
    {
        _owed.TryGetValue(damageable, out float owed);

        owed += Context.Boost.Roll(Damage * Row.Rate);

        int whole = Mathf.FloorToInt(owed);

        _owed[damageable] = owed - whole;

        return whole;
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
