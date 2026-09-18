using UnityEngine;

public class ProjectileDelivery : WeaponDelivery
{
    private readonly SpawnPool<Ammo> _ammoPool;
    private readonly SpawnPool<ExplosionAnimation> _hitEffectPool;
    private readonly ShotAudio _audio;

    public ProjectileDelivery(WeaponRow row, Transform[] mounts, int barrels, PoolRegistry pools, ShotAudio audio)
        : base(row, mounts, barrels, pools)
    {
        _ammoPool = pools.GetPool(row.Projectile);
        _hitEffectPool = pools.GetPool(row.Projectile.HitEffectPrefab);
        _audio = audio;
    }

    protected override void OnFire()
    {
        for (int barrel = 0; barrel < Barrels; barrel++)
        {
            Transform mount = GetMount(barrel);
            Ammo ammo = _ammoPool.Get(mount.position, GetRotation(barrel));

            ammo.Init(_hitEffectPool);
            ammo.SetDamage(Row.Damage);
            ammo.Fly(Row.ProjectileSpeed);

            if (barrel == 0)
                _audio.Play(ammo.ShotSound, mount.position);
        }
    }
}
