using UnityEngine;

public class BeamDelivery : WeaponDelivery
{
    private const float MinSoundInterval = 0.2f;
    private const float TargetMemory = 0.15f;

    private readonly SpawnPool<ExplosionAnimation> _hitEffectPool;
    private readonly SpawnPool<BeamFlash> _beamPool;
    private readonly RaycastHit2D[] _hits = new RaycastHit2D[1];

    private float _soundLeft;
    private float _burnLeft;
    private float _targetAge;

    public BeamDelivery(WeaponRow row, WeaponContext context) : base(row, context)
    {
        if (row.HitEffect != null)
            _hitEffectPool = context.Pools.GetPool(row.HitEffect);

        if (row.Beam != null)
            _beamPool = context.Pools.GetPool(row.Beam);
    }

    protected Transform Target { get; private set; }

    protected override void OnFire()
    {
        ForgetTarget();

        for (int barrel = 0; barrel < Barrels; barrel++)
            Shoot(barrel);
    }

    protected void ForgetTarget()
    {
        Target = null;
        _targetAge = 0f;
    }

    protected void Shoot(int barrel)
    {
        Transform mount = GetMount(barrel);
        Vector2 direction = GetRotation(barrel) * Vector2.right;
        float reach = GetReach();

        int count = Physics2D.Raycast(mount.position, direction, Context.TargetFilter, _hits, reach);
        bool isSeen = count > 0 && ScreenArea.Contains(Context.Camera, _hits[0].point);
        Vector2 end = isSeen
            ? _hits[0].point
            : (Vector2)mount.position + direction * reach;

        BeamFlash flash = Draw(mount.position, end, barrel);

        if (isSeen == false)
            return;

        RaycastHit2D hit = _hits[0];

        if (hit.collider.TryGetComponent(out IDamageable damageable) == false)
            damageable = hit.collider.GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(Row.Damage);
            Burn(flash, hit.point);

            if (Target == null)
                Target = hit.collider.transform;
        }

        if (_hitEffectPool == null)
            return;

        ExplosionAnimation effect = _hitEffectPool.Get(hit.point);

        effect.Play();
    }

    private BeamFlash Draw(Vector3 from, Vector3 to, int barrel)
    {
        if (_beamPool == null)
            return null;

        BeamFlash flash = _beamPool.Get(from);

        Paint(flash);
        flash.Show(from, to);

        if (barrel != 0 || _soundLeft > 0f)
            return flash;

        _soundLeft = MinSoundInterval;
        Context.Audio.Play(flash.ShotSound, from);

        return flash;
    }

    private void Burn(BeamFlash flash, Vector2 point)
    {
        if (flash == null || _burnLeft > 0f)
            return;

        Sound sound = flash.HitSound;

        if (sound.Clip == null)
            return;

        _burnLeft = sound.Clip.length;
        Context.Audio.Play(sound, point);
    }

    protected override void OnTick(float deltaTime)
    {
        if (_soundLeft > 0f)
            _soundLeft -= deltaTime;

        if (_burnLeft > 0f)
            _burnLeft -= deltaTime;

        if (Target == null)
            return;

        _targetAge += deltaTime;

        if (_targetAge > TargetMemory)
            ForgetTarget();
    }

    protected virtual void Paint(BeamFlash flash)
    {
    }

    private float GetReach()
    {
        return ScreenArea.GetRightEdge(Context.Camera) - ScreenArea.GetLeftEdge(Context.Camera);
    }
}
