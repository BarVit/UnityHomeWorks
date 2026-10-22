using System.Collections.Generic;
using UnityEngine;

public class OrbDelivery : ProjectileDelivery
{
    private const float ChargeDuration = 0.3f;
    private const float ArcReach = 6f;
    private const float FarShare = 0.5f;
    private const float GoneShare = 0.9f;

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

    private bool IsGone(Vector2 centre, float radius)
    {
        return centre.x - radius * (GoneShare * 2f - 1f) >= ScreenArea.GetRightEdge(Context.Camera);
    }

    private float GetDamageRate(Ammo orb)
    {
        int damage = GetDamage(orb.Barrel);

        return orb.TryGetComponent(out OrbGrowth growth) ? growth.GetDamageRate(damage) : damage;
    }

    private void Deal(IDamageable damageable, float damage, int barrel)
    {
        _owed.TryGetValue(damageable, out float owed);
        owed += RollDamage(damage, barrel, out bool isCrit);

        if (isCrit && damageable is Component target)
            ShowCrit(target.transform.position);

        int whole = Mathf.FloorToInt(owed);

        if (whole > 0)
        {
            DamageLog.SetSource(Row.Id + "#" + barrel);
            damageable.TakeDamage(whole);
        }

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
        int count = IsGone(centre, radius)
            ? 0
            : Physics2D.OverlapCircle(centre, radius * ArcReach, Context.TargetFilter, _touched);
        int ringed = 0;
        bool isTouching = false;

        for (int i = 0; i < count; i++)
        {
            IDamageable damageable = FindDamageable(_touched[i]);

            if (damageable == null || damageable.IsVulnerable == false)
                continue;

            float distance = Vector2.Distance(centre, _touched[i].transform.position);

            if (distance > radius * ArcReach)
                continue;

            bool isInside = distance <= radius;
            float share = Mathf.Lerp(1f, FarShare, Mathf.InverseLerp(radius, radius * ArcReach, distance));

            Deal(damageable, tickDamage * share, orb.Barrel);
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

        if (isTouching && orb.TryScore())
            CountHit();

        OrbArcSound sound = orb.GetComponentInChildren<OrbArcSound>();

        if (sound != null)
            sound.SetTouching(isTouching);
    }
}
