using UnityEngine;

public class OrbDelivery : ProjectileDelivery
{
    private readonly Collider2D[] _touched = new Collider2D[8];

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

    private void Burn(Ammo orb)
    {
        int count = Physics2D.OverlapCircleNonAlloc(orb.transform.position, Row.Area, _touched, Context.TargetLayers);

        for (int i = 0; i < count; i++)
        {
            if (_touched[i].TryGetComponent(out IDamageable damageable) == false)
                damageable = _touched[i].GetComponentInParent<IDamageable>();

            if (damageable != null)
                damageable.TakeDamage(Row.Damage);
        }
    }
}
