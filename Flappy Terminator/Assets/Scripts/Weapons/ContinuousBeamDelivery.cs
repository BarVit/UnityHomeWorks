using UnityEngine;

public class ContinuousBeamDelivery : BeamDelivery
{
    private static readonly Color CoolColor = new Color(0.25f, 1f, 0.35f);
    private static readonly Color WarmColor = new Color(1f, 0.92f, 0.2f);
    private static readonly Color HotColor = new Color(1f, 0.2f, 0.12f);

    public ContinuousBeamDelivery(WeaponRow row, WeaponContext context) : base(row, context)
    {
    }

    public override Transform LockTarget => Target;

    protected override void OnFire()
    {
        ForgetTarget();

        for (int barrel = 0; barrel < Barrels; barrel++)
            Shoot(barrel);

        HoldLoop();
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
