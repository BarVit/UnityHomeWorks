using System;
using UnityEngine;

public static class WeaponFactory
{
    public static WeaponDelivery Create(WeaponRow row, Transform[] mounts, int barrels,
        PoolRegistry pools, ShotAudio audio)
    {
        switch (row.Delivery)
        {
            case DeliveryKind.Projectile:
                return new ProjectileDelivery(row, mounts, barrels, pools, audio);

            default:
                throw new NotSupportedException("Механика доставки " + row.Delivery + " ещё не реализована");
        }
    }
}
