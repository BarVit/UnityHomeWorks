using UnityEngine;

public class BeamDelivery : WeaponDelivery
{
    private const float MinSoundInterval = 0.2f;
    private const float TargetMemory = 0.15f;

    private readonly SpawnPool<ExplosionAnimation> _hitEffectPool;
    private readonly SpawnPool<BeamFlash> _beamPool;
    private readonly SpawnPool<ExplosionAnimation> _piercePool;
    private readonly SpawnPool<ExplosionAnimation> _worldEffectPool;
    private readonly RaycastHit2D[] _hits = new RaycastHit2D[16];
    private readonly IDamageable[] _struck = new IDamageable[16];
    private readonly RaycastHit2D[] _worldHits = new RaycastHit2D[1];
    private readonly RaycastHit2D[] _exitHits = new RaycastHit2D[8];
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
        Vector2 far = from + direction * reach;
        Vector2 end = from + direction * open;

        int count = Physics2D.Raycast(from, direction, Context.TargetFilter, _hits, open);
        int struck = 0;
        Vector2 last = end;

        for (int i = 0; i < count; i++)
        {
            RaycastHit2D hit = _hits[i];

            if (ScreenArea.Contains(Context.Camera, hit.point) == false)
                break;

            IDamageable damageable = FindDamageable(hit.collider);

            if (damageable != null && IsStruck(damageable, struck))
                continue;

            Strike(hit, damageable, rotation, far);
            _struck[struck] = damageable;
            struck++;
            last = hit.point;

            if (_piercePool == null || struck == _struck.Length)
                break;
        }

        Remember(Draw(from, last, barrel));

        if (struck == 0 && open < reach)
            NoteWorldHit(end, rotation);
    }

    private IDamageable FindDamageable(Collider2D collider)
    {
        if (collider.TryGetComponent(out IDamageable damageable) == false)
            damageable = collider.GetComponentInParent<IDamageable>();

        return damageable;
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

    private void Strike(RaycastHit2D hit, IDamageable damageable, Quaternion rotation, Vector2 far)
    {
        if (damageable != null && _piercePool != null)
            ShowPierce(GetExit(hit.collider, hit.point, far), rotation);

        if (damageable != null)
        {
            damageable.TakeDamage(Damage);
            NoteTargetHit(hit.point);

            if (Target == null)
                Target = hit.collider.transform;
        }

        if (_hitEffectPool == null)
            return;

        ExplosionAnimation effect = _hitEffectPool.Get(hit.point, rotation);

        effect.Play();
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
