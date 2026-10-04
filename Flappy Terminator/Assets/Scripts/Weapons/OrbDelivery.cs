using System.Collections.Generic;
using UnityEngine;

public class OrbDelivery : ProjectileDelivery
{
    private const float ChargeDuration = 0.3f;
    private const float ArcReach = 4f;
    private const float RingShare = 0.5f;

    private readonly Collider2D[] _touched = new Collider2D[16];
    private readonly Transform[] _ringed = new Transform[16];
    private readonly Dictionary<IDamageable, float> _owed = new Dictionary<IDamageable, float>();

    private readonly SpawnPool<ExplosionAnimation> _sparkPool;

    private float _tickLeft;
    private Ammo _charging;
    private float _chargeLeft;

    public OrbDelivery(WeaponRow row, WeaponContext context) : base(row, context)
    {
        if (row.HitEffect != null)
            _sparkPool = context.Pools.GetPool(row.HitEffect);
    }

    protected override Ammo Launch(int barrel)
    {
        Ammo orb = base.Launch(barrel);

        orb.MakePersistent();
        orb.Stop();

        if (orb.TryGetComponent(out OrbGrowth growth))
            growth.Charge(ChargeDuration);

        _charging = orb;
        _chargeLeft = ChargeDuration;

        return orb;
    }

    protected override void OnTick(float deltaTime)
    {
        Charge(deltaTime);

        _tickLeft -= deltaTime;

        if (_tickLeft > 0f)
            return;

        _tickLeft = Row.TickInterval;

        AmmoPool.ForEachActive(Burn);
    }

    private void Charge(float deltaTime)
    {
        if (_charging == null)
            return;

        if (_charging.gameObject.activeInHierarchy == false)
        {
            _charging = null;

            return;
        }

        _charging.transform.SetPositionAndRotation(GetMount(0).position, GetRotation(0));
        _chargeLeft -= deltaTime;

        if (_chargeLeft > 0f)
            return;

        _charging.Fly(ProjectileSpeed);
        _charging = null;
    }

    private void Spark(Vector3 position)
    {
        if (_sparkPool == null)
            return;

        ExplosionAnimation spark = _sparkPool.Get(position);

        spark.Play();
    }

    private float GetDamageRate(Ammo orb)
    {
        return orb.TryGetComponent(out OrbGrowth growth) ? growth.GetDamageRate(Row.Damage) : Row.Damage;
    }

    private void Deal(IDamageable damageable, float damage)
    {
        _owed.TryGetValue(damageable, out float owed);
        owed += damage;

        int whole = Mathf.FloorToInt(owed);

        if (whole > 0)
            damageable.TakeDamage(whole);

        _owed[damageable] = owed - whole;
    }

    private float GetRadius(Ammo orb)
    {
        SpriteRenderer renderer = orb.GetComponentInChildren<SpriteRenderer>();

        return renderer == null ? Row.Area : renderer.bounds.extents.x;
    }

    private void Burn(Ammo orb)
    {
        float radius = GetRadius(orb);
        float tickDamage = GetDamageRate(orb) * Row.TickInterval;
        Vector2 centre = orb.transform.position;
        int count = Physics2D.OverlapCircle(centre, radius * ArcReach, Context.TargetFilter, _touched);
        int ringed = 0;
        bool isTouching = false;

        for (int i = 0; i < count; i++)
        {
            if (_touched[i].TryGetComponent(out IDamageable damageable) == false)
                damageable = _touched[i].GetComponentInParent<IDamageable>();

            if (damageable == null)
                continue;

            float distance = Vector2.Distance(centre, _touched[i].transform.position);

            if (distance > radius * ArcReach)
                continue;

            bool isInside = distance <= radius;

            Deal(damageable, isInside ? tickDamage : tickDamage * RingShare);
            Spark(_touched[i].transform.position);
            isTouching = true;

            if (isInside)
                continue;

            _ringed[ringed] = _touched[i].transform;
            ringed++;
        }

        PlasmaTether tether = orb.GetComponentInChildren<PlasmaTether>();

        if (tether != null)
            tether.Hold(_ringed, ringed);

        OrbArcSound sound = orb.GetComponentInChildren<OrbArcSound>();

        if (sound != null)
            sound.SetTouching(isTouching);
    }
}
