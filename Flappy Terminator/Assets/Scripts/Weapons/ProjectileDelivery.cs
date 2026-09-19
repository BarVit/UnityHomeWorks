using UnityEngine;

public class ProjectileDelivery : WeaponDelivery
{
    private readonly SpawnPool<Ammo> _ammoPool;
    private readonly SpawnPool<ExplosionAnimation> _hitEffectPool;

    public ProjectileDelivery(WeaponRow row, WeaponContext context) : base(row, context)
    {
        _ammoPool = context.Pools.GetPool(row.Projectile);
        _hitEffectPool = context.Pools.GetPool(row.Projectile.HitEffectPrefab);
    }

    protected SpawnPool<Ammo> AmmoPool => _ammoPool;

    protected override void OnFire()
    {
        for (int barrel = 0; barrel < Barrels; barrel++)
            Launch(barrel);
    }

    protected virtual Ammo Launch(int barrel)
    {
        Transform mount = GetMount(barrel);
        Ammo ammo = _ammoPool.Get(mount.position, GetRotation(barrel));

        ammo.Init(_hitEffectPool);
        ammo.SetDamage(Row.Damage);
        ammo.Fly(ProjectileSpeed);

        if (barrel == 0)
            Context.Audio.Play(ammo.ShotSound, mount.position);

        return ammo;
    }
}
