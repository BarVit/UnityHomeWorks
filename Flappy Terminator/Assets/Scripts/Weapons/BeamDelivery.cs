using UnityEngine;

public class BeamDelivery : WeaponDelivery
{
    private const float MinSoundInterval = 0.2f;

    private readonly SpawnPool<ExplosionAnimation> _hitEffectPool;
    private readonly SpawnPool<BeamFlash> _beamPool;
    private readonly RaycastHit2D[] _hits = new RaycastHit2D[1];

    private float _soundLeft;

    public BeamDelivery(WeaponRow row, WeaponContext context) : base(row, context)
    {
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
    }

    protected void Shoot(int barrel)
    {
        Transform mount = GetMount(barrel);
        Vector2 direction = GetRotation(barrel) * Vector2.right;
        float reach = GetReach();

        int count = Physics2D.Raycast(mount.position, direction, Context.TargetFilter, _hits, reach);
        Vector2 end = count == 0
            ? (Vector2)mount.position + direction * reach
            : _hits[0].point;

        Draw(mount.position, end, barrel);

        if (count == 0)
            return;

        RaycastHit2D hit = _hits[0];

        if (hit.collider.TryGetComponent(out IDamageable damageable) == false)
            damageable = hit.collider.GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(Row.Damage);

            if (Target == null)
                Target = hit.collider.transform;
        }

        ExplosionAnimation effect = _hitEffectPool.Get(hit.point);

        effect.Play();
    }

    private void Draw(Vector3 from, Vector3 to, int barrel)
    {
        if (_beamPool == null)
            return;

        BeamFlash flash = _beamPool.Get(from);

        flash.Show(from, to);

        if (barrel != 0 || _soundLeft > 0f)
            return;

        _soundLeft = MinSoundInterval;
        Context.Audio.Play(flash.ShotSound, from);
    }

    protected override void OnTick(float deltaTime)
    {
        if (_soundLeft > 0f)
            _soundLeft -= deltaTime;
    }

    private float GetReach()
    {
        return ScreenArea.GetRightEdge(Context.Camera) - ScreenArea.GetLeftEdge(Context.Camera);
    }
}
