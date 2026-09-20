using UnityEngine;

public class OrbDelivery : ProjectileDelivery
{
    private readonly Collider2D[] _touched = new Collider2D[16];
    private readonly Collider2D[] _reached = new Collider2D[8];

    private float _tickLeft;

    public OrbDelivery(WeaponRow row, WeaponContext context) : base(row, context)
    {
    }

    protected override Ammo Launch(int barrel)
    {
        Ammo orb = base.Launch(barrel);

        orb.MakePersistent();

        return orb;
    }

    protected override void OnTick(float deltaTime)
    {
        _tickLeft -= deltaTime;

        if (_tickLeft > 0f)
            return;

        _tickLeft = Row.TickInterval;

        AmmoPool.ForEachActive(Burn);
    }

    private float GetRadius(Ammo orb)
    {
        SpriteRenderer renderer = orb.GetComponentInChildren<SpriteRenderer>();

        return renderer == null ? Row.Area : renderer.bounds.extents.x;
    }

    private void Burn(Ammo orb)
    {
        float radius = GetRadius(orb);
        Vector2 centre = orb.transform.position;
        int count = Physics2D.OverlapCircle(centre, radius * 2f, Context.TargetFilter, _touched);
        int latched = 0;

        for (int i = 0; i < count; i++)
        {
            if (_touched[i].TryGetComponent(out IDamageable damageable) == false)
                damageable = _touched[i].GetComponentInParent<IDamageable>();

            if (damageable == null)
                continue;

            if (Vector2.Distance(centre, _touched[i].transform.position) <= radius)
            {
                damageable.TakeDamage(Row.Damage);

                continue;
            }

            damageable.TakeDamage(Row.Damage / 2);
            _reached[latched] = _touched[i];
            latched++;

            if (latched == _reached.Length)
                break;
        }

        Latch(orb, latched);
    }

    private void Latch(Ammo orb, int count)
    {
        PlasmaArcs arcs = orb.GetComponentInChildren<PlasmaArcs>();

        if (arcs == null)
            return;

        arcs.Latch(_reached, count);
    }
}
