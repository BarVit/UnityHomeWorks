using UnityEngine;

public class ProjectileDelivery : WeaponDelivery
{
    private readonly SpawnPool<Ammo> _ammoPool;
    private readonly SpawnPool<ExplosionAnimation> _hitEffectPool;

    private float _hitSoundUntil = -1f;

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

        ammo.gameObject.layer = Context.AmmoLayer;
        ammo.Init(_hitEffectPool);
        ammo.MarkBarrel(barrel);
        ammo.SetDamage(RollDamage(barrel, out bool isCrit), isCrit);
        ammo.Fly(ProjectileSpeed);
        ammo.Struck += OnStruck;

        return ammo;
    }

    protected virtual float GetHitSoundInterval(Sound sound)
    {
        return 0f;
    }

    private void OnStruck(Ammo ammo)
    {
        bool isTarget = IsTarget(ammo.StruckObject);

        if (isTarget && ammo.IsCrit)
            ShowCrit(ammo.transform.position, ammo.Damage);

        Sound sound = isTarget ? ammo.HitSound : ammo.WorldHitSound;

        if (sound.Clip == null || Time.time <= _hitSoundUntil)
            return;

        _hitSoundUntil = Time.time + GetHitSoundInterval(sound);
        Context.HitAudio.Play(sound, ammo.transform.position);
    }

    private bool IsTarget(GameObject struck)
    {
        return struck != null && struck.GetComponentInParent<IDamageable>(true) != null;
    }
}
