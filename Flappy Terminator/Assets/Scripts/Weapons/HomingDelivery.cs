public class HomingDelivery : ProjectileDelivery
{
    public HomingDelivery(WeaponRow row, WeaponContext context) : base(row, context)
    {
    }

    protected override Ammo Launch(int barrel)
    {
        Ammo ammo = base.Launch(barrel);
        WeaponBoost boost = Context.GetBoost(barrel);
        float turnRate = Row.TurnRate * boost.TurnScale;

        HomingSteer.Guide(ammo, turnRate, ProjectileSpeed * boost.SpeedScale, Context.TargetFilter);

        return ammo;
    }
}
