using UnityEngine;

public class BeamDelivery : WeaponDelivery
{
    private readonly SpawnPool<ExplosionAnimation> _hitEffectPool;
    private readonly RaycastHit2D[] _hits = new RaycastHit2D[1];

    public BeamDelivery(WeaponRow row, WeaponContext context) : base(row, context)
    {
        _hitEffectPool = context.Pools.GetPool(row.HitEffect);
    }

    protected override void OnFire()
    {
        for (int barrel = 0; barrel < Barrels; barrel++)
            Shoot(barrel);
    }

    protected void Shoot(int barrel)
    {
        Transform mount = GetMount(barrel);
        Vector2 direction = GetRotation(barrel) * Vector2.right;
        float reach = GetReach();

        int count = Physics2D.RaycastNonAlloc(mount.position, direction, _hits, reach, Context.TargetLayers);

        if (count == 0)
            return;

        RaycastHit2D hit = _hits[0];

        if (hit.collider.TryGetComponent(out IDamageable damageable) == false)
            damageable = hit.collider.GetComponentInParent<IDamageable>();

        if (damageable != null)
            damageable.TakeDamage(Row.Damage);

        ExplosionAnimation effect = _hitEffectPool.Get(hit.point);

        effect.Play();
    }

    private float GetReach()
    {
        return ScreenArea.GetRightEdge(Context.Camera) - ScreenArea.GetLeftEdge(Context.Camera);
    }
}
