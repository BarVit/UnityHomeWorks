using UnityEngine;

public class ProjectileDelivery : WeaponDelivery
{
    private readonly SpawnPool<Ammo> _ammoPool;
    private readonly SpawnPool<ExplosionAnimation> _hitEffectPool;

    public ProjectileDelivery(WeaponRow row, WeaponContext context) : base(row, context)
    {
        _ammoPool = context.Pools.GetPool(row.Projectile);

        if (row.HitEffect != null)
            _hitEffectPool = context.Pools.GetPool(row.HitEffect);
    }

    protected SpawnPool<Ammo> AmmoPool => _ammoPool;

    protected void ShowHit(Vector2 point)
    {
        if (_hitEffectPool == null)
            return;

        ExplosionAnimation hitEffect = _hitEffectPool.Get(point);

        hitEffect.Play();
    }

    protected override void OnFire()
    {
        for (int barrel = 0; barrel < Barrels; barrel++)
            Launch(barrel);
    }

    protected virtual Ammo Launch(int barrel)
    {
        Ammo ammo = Spawn(barrel);

        if (barrel == 0)
            Context.Audio.Play(ammo.ShotSound, GetMount(barrel).position);

        return ammo;
    }

    protected Ammo Spawn(int barrel)
    {
        Transform mount = GetMount(barrel);
        Ammo ammo = _ammoPool.Get(mount.position, GetRotation(barrel));

        ShowShot(barrel);

        ammo.Init(_hitEffectPool);
        ammo.SetDamage(Row.Damage);
        ammo.Fly(ProjectileSpeed);

        return ammo;
    }
}
