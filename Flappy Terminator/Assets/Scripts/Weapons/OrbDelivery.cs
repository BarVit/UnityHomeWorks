using System.Collections.Generic;
using UnityEngine;

public class OrbDelivery : ProjectileDelivery
{
    private const float ChargeDuration = 0.3f;
    private const float ArcReach = 4.5f;
    private const float ArcFalloff = 0.5f;
    private const float GoneShare = 0.9f;
    private const float ArcGrabTime = 0.1f;

    private readonly Collider2D[] _touched = new Collider2D[16];
    private readonly Transform[] _ringed = new Transform[16];
    private readonly Dictionary<IDamageable, float> _owed = new Dictionary<IDamageable, float>();
    private readonly Dictionary<IDamageable, ArcGrab> _grabs = new Dictionary<IDamageable, ArcGrab>();
    private readonly List<IDamageable> _released = new List<IDamageable>();

    private readonly SpawnPool<ExplosionAnimation> _sparkPool;

    private float _tickLeft;
    private Ammo _charging;
    private float _chargeLeft;
    private float _clock;

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
        {
            growth.SetSize(Context.GetBoost(barrel).OrbSizeScale);
            growth.Charge(ChargeDuration);
        }

        _charging = orb;
        _chargeLeft = ChargeDuration;

        return orb;
    }

    protected override void OnTick(float deltaTime)
    {
        Charge(deltaTime);

        _clock += deltaTime;
        ReleaseGrabbed();
        _tickLeft -= deltaTime;

        if (_tickLeft > 0f)
            return;

        _tickLeft = Row.TickInterval;

        foreach (ArcGrab grab in _grabs.Values)
            grab.Forget();

        AmmoPool.ForEachActive(Burn);
        DropUnseen();
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

    private void Arc(IDamageable damageable, Vector3 position, float damage, int barrel)
    {
        if (_grabs.TryGetValue(damageable, out ArcGrab grab) == false)
        {
            grab = new ArcGrab(_clock);
            _grabs.Add(damageable, grab);
        }

        grab.See();

        if (_clock - grab.GrabbedAt < ArcGrabTime)
        {
            grab.Delay(damage * GetShareAfterGrab(), barrel);

            return;
        }

        Deal(damageable, damage, barrel);
        Spark(position);
    }

    private float GetShareAfterGrab()
    {
        return Row.TickInterval <= 0f ? 0f : 1f - Mathf.Clamp01(ArcGrabTime / Row.TickInterval);
    }

    private void ReleaseGrabbed()
    {
        foreach (KeyValuePair<IDamageable, ArcGrab> pair in _grabs)
        {
            ArcGrab grab = pair.Value;

            if (grab.Pending <= 0f || _clock - grab.GrabbedAt < ArcGrabTime)
                continue;

            float damage = grab.TakePending();

            if (pair.Key is Component target && target.gameObject.activeInHierarchy && pair.Key.IsVulnerable)
            {
                Deal(pair.Key, damage, grab.Barrel);
                Spark(target.transform.position);
            }
        }
    }

    private void DropUnseen()
    {
        _released.Clear();

        foreach (KeyValuePair<IDamageable, ArcGrab> pair in _grabs)
        {
            if (pair.Value.IsSeen == false)
                _released.Add(pair.Key);
        }

        foreach (IDamageable damageable in _released)
            _grabs.Remove(damageable);
    }

    private float GetRadius(Ammo orb)
    {
        SpriteRenderer renderer = orb.GetComponentInChildren<SpriteRenderer>();

        return renderer == null ? Row.Area : renderer.bounds.extents.x;
    }

    private void Burn(Ammo orb)
    {
        float radius = GetRadius(orb);
        WeaponBoost boost = Context.GetBoost(orb.Barrel);
        float reach = radius * ArcReach * boost.ArcLengthScale;
        float surfaceShare = boost.ArcSurfaceShare;
        float tickDamage = GetDamage(orb.Barrel) * Row.TickInterval;
        Vector2 centre = orb.transform.position;
        int count = IsGone(centre, radius)
            ? 0
            : Physics2D.OverlapCircle(centre, reach, Context.TargetFilter, _touched);
        int ringed = 0;
        bool isTouching = false;

        for (int i = 0; i < count; i++)
        {
            IDamageable damageable = FindDamageable(_touched[i]);

            if (damageable == null || damageable.IsVulnerable == false)
                continue;

            float distance = Vector2.Distance(centre, _touched[i].transform.position);

            if (distance > reach)
                continue;

            bool isInside = distance <= radius;
            float share = isInside ? 1f
                : Mathf.Lerp(surfaceShare, surfaceShare - ArcFalloff, Mathf.InverseLerp(radius, reach, distance));

            isTouching = true;

            if (isInside)
            {
                Deal(damageable, tickDamage * share, orb.Barrel);
                Spark(_touched[i].transform.position);

                continue;
            }

            Arc(damageable, _touched[i].transform.position, tickDamage * share, orb.Barrel);

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
