using UnityEngine;

public class BeamDelivery : WeaponDelivery
{
    private const float MinSoundInterval = 0.2f;
    private const float TargetMemory = 0.15f;

    private readonly SpawnPool<ExplosionAnimation> _hitEffectPool;
    private readonly SpawnPool<BeamFlash> _beamPool;
    private readonly SpawnPool<ExplosionAnimation> _piercePool;
    private readonly SpawnPool<ExplosionAnimation> _worldEffectPool;
    private readonly RaycastHit2D[] _hits = new RaycastHit2D[1];
    private readonly RaycastHit2D[] _worldHits = new RaycastHit2D[1];
    private readonly RaycastHit2D[] _exitHits = new RaycastHit2D[8];

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
    }

    protected Transform Target { get; private set; }

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
        Transform mount = GetMount(barrel);
        Vector2 direction = GetRotation(barrel) * Vector2.right;
        float reach = GetReach();

        Vector2 far = (Vector2)mount.position + direction * reach;

        int count = Physics2D.Raycast(mount.position, direction, Context.TargetFilter, _hits, reach);
        bool isSeen = count > 0 && ScreenArea.Contains(Context.Camera, _hits[0].point);

        if (isSeen == false)
        {
            Remember(Draw(mount.position, far, barrel));
            NoteWorldHit(mount.position, direction, reach);

            return;
        }

        RaycastHit2D hit = _hits[0];

        if (hit.collider.TryGetComponent(out IDamageable damageable) == false)
            damageable = hit.collider.GetComponentInParent<IDamageable>();

        bool isPierced = damageable != null && _piercePool != null;

        Remember(Draw(mount.position, isPierced ? far : hit.point, barrel));

        if (isPierced)
            ShowPierce(GetExit(hit.collider, hit.point, far), GetRotation(barrel));

        if (damageable != null)
        {
            damageable.TakeDamage(Row.Damage);
            NoteTargetHit(hit.point);

            if (Target == null)
                Target = hit.collider.transform;
        }

        if (_hitEffectPool == null)
            return;

        ExplosionAnimation effect = _hitEffectPool.Get(hit.point);

        effect.Play();
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

    private void NoteWorldHit(Vector2 from, Vector2 direction, float reach)
    {
        if (_hasWorldHit && _worldEffectPool == null)
            return;

        int count = Physics2D.Raycast(from, direction, Context.WorldFilter, _worldHits, reach);

        if (count == 0 || ScreenArea.Contains(Context.Camera, _worldHits[0].point) == false)
            return;

        ShowWorldHit(_worldHits[0].point);

        if (_hasWorldHit)
            return;

        _hasWorldHit = true;
        _worldHitPoint = _worldHits[0].point;
    }

    private void ShowWorldHit(Vector2 point)
    {
        if (_worldEffectPool == null)
            return;

        ExplosionAnimation effect = _worldEffectPool.Get(point);

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
