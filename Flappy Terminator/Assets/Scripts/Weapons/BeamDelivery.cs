using UnityEngine;

public class BeamDelivery : WeaponDelivery
{
    private const float MinSoundInterval = 0.2f;
    private const float TargetMemory = 0.15f;
    private const float EdgeInset = 0.05f;
    private const int EdgeSteps = 3;

    private readonly SpawnPool<ExplosionAnimation> _hitEffectPool;
    private readonly SpawnPool<BeamFlash> _beamPool;
    private readonly SpawnPool<ExplosionAnimation> _piercePool;
    private readonly SpawnPool<ExplosionAnimation> _worldEffectPool;
    private readonly RaycastHit2D[] _hits = new RaycastHit2D[16];
    private readonly IDamageable[] _struck = new IDamageable[16];
    private readonly RaycastHit2D[] _worldHits = new RaycastHit2D[1];
    private readonly RaycastHit2D[] _exitHits = new RaycastHit2D[8];
    private readonly Collider2D[] _candidates = new Collider2D[32];
    private readonly AimLine[] _sights;

    private BeamFlash _soundFlash;
    private bool _hasTargetHit;
    private bool _hasWorldHit;
    private Vector2 _targetHitPoint;
    private Vector2 _worldHitPoint;
    private float _soundLeft;
    private float _targetAge;

    public BeamDelivery(WeaponRow row, WeaponContext context) : base(row, context)
    {
        if (row.HitEffect != null)
            _hitEffectPool = context.Pools.GetPool(row.HitEffect);

        if (row.Beam != null)
            _beamPool = context.Pools.GetPool(row.Beam);

        if (row.Beam != null && row.Beam.PierceEffect != null)
            _piercePool = context.Pools.GetPool(row.Beam.PierceEffect);

        if (row.Assets != null && row.Assets.HitWorldEffect != null)
            _worldEffectPool = context.Pools.GetPool(row.Assets.HitWorldEffect);

        _sights = new AimLine[Barrels];

        if (row.Beam != null && row.Beam.Sight != null)
            for (int barrel = 0; barrel < Barrels; barrel++)
                _sights[barrel] = CreateSight(row.Beam.Sight, barrel);
    }

    public override bool IsReady => base.IsReady && IsAimed();

    protected Transform Target { get; private set; }

    public override void Dispose()
    {
        foreach (AimLine sight in _sights)
            if (sight != null)
                Object.Destroy(sight.gameObject);
    }

    protected override void OnFire()
    {
        ForgetTarget();
        ForgetHits();

        for (int barrel = 0; barrel < Barrels; barrel++)
            Shoot(barrel);

        SoundHits();
        HoldLoop();
    }

    protected void ForgetTarget()
    {
        Target = null;
        _targetAge = 0f;
    }

    protected void HoldLoop()
    {
        if (_soundFlash == null)
            return;

        if (Context.Loop != null)
            Context.Loop.Hold(_soundFlash.LoopSound);

        if (Context.HitLoop != null && (_hasTargetHit || _hasWorldHit))
            Context.HitLoop.Hold(_soundFlash.HitLoopSound);
    }

    protected void Shoot(int barrel)
    {
        AimLine sight = _sights[barrel];
        Vector2 from = sight == null ? GetMount(barrel).position : sight.From;
        Quaternion rotation = sight == null ? GetRotation(barrel) : sight.Rotation;
        Vector2 direction = rotation * Vector2.right;
        float reach = GetReach();

        if (sight != null)
            sight.Rest();

        float open = GetOpenDistance(from, direction, reach);

        if (TryAutoAim(from, ref direction, open, barrel))
        {
            rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            open = GetOpenDistance(from, direction, reach);
        }

        Vector2 far = from + direction * reach;
        Vector2 end = from + direction * open;

        int count = Physics2D.Raycast(from, direction, Context.TargetFilter, _hits, open);
        int struck = 0;
        bool isOnTarget = false;
        float share = 1f;
        float pierceShare = Context.GetBoost(barrel).PierceShare;

        CountFired();
        Vector2 last = end;

        for (int i = 0; i < count; i++)
        {
            RaycastHit2D hit = _hits[i];

            if (ScreenArea.Contains(Context.Camera, hit.point) == false)
                break;

            IDamageable damageable = FindDamageable(hit.collider);

            if (damageable != null && (damageable.IsVulnerable == false || IsStruck(damageable, struck)))
                continue;

            Strike(hit, damageable, rotation, far, barrel, share);
            isOnTarget |= damageable != null;

            if (damageable != null)
                share *= pierceShare;

            _struck[struck] = damageable;
            struck++;
            last = hit.point;

            if (_piercePool == null || struck == _struck.Length)
                break;
        }

        Remember(Draw(from, last, barrel));

        if (isOnTarget)
            CountHit();

        if (struck == 0 && open < reach)
            NoteWorldHit(end, rotation);
    }

    private bool IsStruck(IDamageable damageable, int struck)
    {
        for (int i = 0; i < struck; i++)
        {
            if (_struck[i] == damageable)
                return true;
        }

        return false;
    }

    private void Strike(RaycastHit2D hit, IDamageable damageable, Quaternion rotation, Vector2 far, int barrel,
        float share)
    {
        if (damageable != null && _piercePool != null)
            ShowPierce(GetExit(hit.collider, hit.point, far), rotation);

        if (damageable != null)
        {
            int damage = GetStrikeDamage(damageable, barrel, out bool isCrit);

            if (share < 1f)
                damage = Mathf.RoundToInt(damage * share);

            if (damage > 0)
            {
                DamageLog.SetSource(Row.Id + "#" + barrel + (isCrit ? " crit" : string.Empty));
                damageable.TakeDamage(damage);

                if (isCrit)
                    ShowStrikeCrit(hit.point, damage);
            }

            NoteTargetHit(hit.point);

            if (Target == null)
                Target = hit.collider.transform;
        }

        if (_hitEffectPool == null)
            return;

        ExplosionAnimation effect = _hitEffectPool.Get(hit.point, rotation);

        effect.Play();
    }

    protected virtual int GetStrikeDamage(IDamageable damageable, int barrel, out bool isCrit)
    {
        return RollDamage(barrel, out isCrit);
    }

    protected virtual void ShowStrikeCrit(Vector2 point, int damage)
    {
        ShowCrit(point, damage);
    }

    private bool TryAutoAim(Vector2 from, ref Vector2 direction, float open, int barrel)
    {
        float limit = Context.GetBoost(barrel).AutoAimDegrees;

        if (limit <= 0f || IsOnTarget(from, direction, open))
            return false;

        Vector2 center = Context.Camera.transform.position;
        Vector2 half = new Vector2(Context.Camera.orthographicSize * Context.Camera.aspect,
            Context.Camera.orthographicSize);
        int count = Physics2D.OverlapArea(center - half, center + half, Context.TargetFilter, _candidates);
        float best = limit;
        bool isFound = false;
        Vector2 aim = direction;

        for (int i = 0; i < count; i++)
        {
            Collider2D candidate = _candidates[i];
            IDamageable damageable = FindDamageable(candidate);

            if (damageable == null || damageable.IsVulnerable == false)
                continue;

            Vector2 edge = GetNearestEdge(candidate, from, direction);
            Vector2 toEdge = edge - from;
            float angle = Vector2.Angle(direction, toEdge);

            if (angle >= best || Vector2.Dot(toEdge, direction) <= 0f
                || ScreenArea.Contains(Context.Camera, edge) == false)
                continue;

            Vector2 inside = Vector2.MoveTowards(edge, candidate.bounds.center, EdgeInset);
            Vector2 toInside = inside - from;

            if (GetOpenDistance(from, toInside.normalized, toInside.magnitude) < toInside.magnitude)
                continue;

            best = angle;
            aim = toInside.normalized;
            isFound = true;
        }

        if (isFound)
            direction = aim;

        return isFound;
    }

    private bool IsOnTarget(Vector2 from, Vector2 direction, float open)
    {
        int count = Physics2D.Raycast(from, direction, Context.TargetFilter, _hits, open);

        for (int i = 0; i < count; i++)
        {
            if (ScreenArea.Contains(Context.Camera, _hits[i].point) == false)
                return false;

            IDamageable damageable = FindDamageable(_hits[i].collider);

            if (damageable != null && damageable.IsVulnerable)
                return true;
        }

        return false;
    }

    private static Vector2 GetNearestEdge(Collider2D candidate, Vector2 from, Vector2 direction)
    {
        Vector2 point = candidate.bounds.center;

        for (int step = 0; step < EdgeSteps; step++)
        {
            float along = Mathf.Max(0f, Vector2.Dot(point - from, direction));

            point = candidate.ClosestPoint(from + direction * along);
        }

        return point;
    }

    private bool IsAimed()
    {
        if (Row.AimTime <= 0f)
            return true;

        foreach (AimLine sight in _sights)
            if (sight != null && sight.AimedTime < Row.AimTime)
                return false;

        return true;
    }

    private AimLine CreateSight(AimLine prefab, int barrel)
    {
        Transform mount = GetMount(barrel);
        AimLine sight = Object.Instantiate(prefab, mount);

        sight.Follow(mount, Quaternion.Inverse(mount.rotation) * GetRotation(barrel));

        return sight;
    }

    private void Remember(BeamFlash flash)
    {
        if (flash != null)
            _soundFlash = flash;
    }

    private Vector2 GetExit(Collider2D struck, Vector2 entry, Vector2 far)
    {
        Vector2 back = entry - far;
        int count = Physics2D.Raycast(far, back.normalized, Context.TargetFilter, _exitHits, back.magnitude);

        for (int i = 0; i < count; i++)
            if (_exitHits[i].collider == struck)
                return _exitHits[i].point;

        return entry;
    }

    private void ShowPierce(Vector2 exit, Quaternion rotation)
    {
        ExplosionAnimation effect = _piercePool.Get(exit, rotation);

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

    protected void ForgetHits()
    {
        _hasTargetHit = false;
        _hasWorldHit = false;
    }

    private void NoteTargetHit(Vector2 point)
    {
        if (_hasTargetHit)
            return;

        _hasTargetHit = true;
        _targetHitPoint = point;
    }

    private float GetOpenDistance(Vector2 from, Vector2 direction, float reach)
    {
        int count = Physics2D.Raycast(from, direction, Context.WorldFilter, _worldHits, reach);

        if (count == 0 || ScreenArea.Contains(Context.Camera, _worldHits[0].point) == false)
            return reach;

        return _worldHits[0].distance;
    }

    private void NoteWorldHit(Vector2 point, Quaternion rotation)
    {
        ShowWorldHit(point, rotation);

        if (_hasWorldHit)
            return;

        _hasWorldHit = true;
        _worldHitPoint = point;
    }

    private void ShowWorldHit(Vector2 point, Quaternion rotation)
    {
        if (_worldEffectPool == null)
            return;

        ExplosionAnimation effect = _worldEffectPool.Get(point, rotation);

        effect.Play();
    }

    private void SoundHits()
    {
        if (_soundFlash == null)
            return;

        if (_hasTargetHit)
            Context.HitAudio.Play(_soundFlash.HitSound, _targetHitPoint);
        else if (_hasWorldHit)
            Context.HitAudio.Play(_soundFlash.WorldHitSound, _worldHitPoint);
    }

    protected override void OnTick(float deltaTime)
    {
        if (_soundLeft > 0f)
            _soundLeft -= deltaTime;

        if (Context.Loop != null)
            Context.Loop.Tick(deltaTime);

        if (Context.HitLoop != null)
            Context.HitLoop.Tick(deltaTime);

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
