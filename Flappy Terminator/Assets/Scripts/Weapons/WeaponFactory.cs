using System;

public static class WeaponFactory
{
    public static WeaponDelivery Create(WeaponRow row, WeaponContext context)
    {
        switch (row.Delivery)
        {
            case DeliveryKind.Projectile:
                return new ProjectileDelivery(row, context);

            case DeliveryKind.Homing:
                return new HomingDelivery(row, context);

            case DeliveryKind.Beam:
                return new BeamDelivery(row, context);

            case DeliveryKind.ContinuousBeam:
                return new ContinuousBeamDelivery(row, context);

            case DeliveryKind.Orb:
                return new OrbDelivery(row, context);

            case DeliveryKind.Impulse:
                return new ImpulseDelivery(row, context);

            case DeliveryKind.Torpedo:
                return new TorpedoDelivery(row, context);

            default:
                throw new NotSupportedException("Неизвестная механика доставки " + row.Delivery);
        }
    }
}
