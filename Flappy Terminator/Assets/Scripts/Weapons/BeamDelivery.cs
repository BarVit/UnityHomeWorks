using UnityEngine;

public class BeamDelivery : WeaponDelivery
{
    private const float MinSoundInterval = 0.2f;
    private const float TargetMemory = 0.15f;

    private readonly SpawnPool<ExplosionAnimation> _hitEffectPool;
    private readonly SpawnPool<BeamFlash> _beamPool;
    private readonly RaycastHit2D[] _hits = new RaycastHit2D[1];
    private readonly RaycastHit2D[] _worldHits = new RaycastHit2D[1];

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
    }

    protected Transform Target { get; private set; }

    protected override void OnFire()
    {
        ForgetTarget();
        ForgetHits();

        for (int barrel = 0; barrel < Barrels; barrel++)
            Shoot(barrel);

        SoundHits();
    }

    protected void ForgetTarget()
    {
        Target = null;
        _targetAge = 0f;
    }

    protected void Shoot(int barrel)
    {
        Transform mount = GetMount(barrel);
        Vector2 direction = GetRotation(barrel) * Vector2.right;
        float reach = GetReach();

        int count = Physics2D.Raycast(mount.position, direction, Context.TargetFilter, _hits, reach);
        bool isSeen = count > 0 && ScreenArea.Contains(Context.Camera, _hits[0].point);
        Vector2 end = isSeen
            ? _hits[0].point
            : (Vector2)mount.position + direction * reach;

        BeamFlash flash = Draw(mount.position, end, barrel);

        if (flash != null)
            _soundFlash = flash;

        if (isSeen == false)
        {
            NoteWorldHit(mount.position, direction, reach);

            return;
        }

        RaycastHit2D hit = _hits[0];

        if (hit.collider.TryGetComponent(out IDamageable damageable) == false)
            damageable = hit.collider.GetComponentInParent<IDamageable>();

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

    private void ForgetHits()
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
        if (_hasWorldHit)
            return;

        int count = Physics2D.Raycast(from, direction, Context.WorldFilter, _worldHits, reach);

        if (count == 0 || ScreenArea.Contains(Context.Camera, _worldHits[0].point) == false)
            return;

        _hasWorldHit = true;
        _worldHitPoint = _worldHits[0].point;
    }

    private void SoundHits()
    {
        if (_soundFlash == null)
            return;

        if (_hasTargetHit)
            Context.Audio.Play(_soundFlash.HitSound, _targetHitPoint);
        else if (_hasWorldHit)
            Context.Audio.Play(_soundFlash.WorldHitSound, _worldHitPoint);
    }

    protected override void OnTick(float deltaTime)
    {
        if (_soundLeft > 0f)
            _soundLeft -= deltaTime;

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
