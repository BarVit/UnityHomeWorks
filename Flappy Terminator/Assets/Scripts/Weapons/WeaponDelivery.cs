using UnityEngine;

public abstract class WeaponDelivery
{
    private readonly WeaponRow _row;
    private readonly WeaponContext _context;
    private readonly int _barrels;

    private float _cooldownLeft;

    protected WeaponDelivery(WeaponRow row, WeaponContext context)
    {
        _row = row;
        _context = context;
        _barrels = Mathf.Clamp(context.Barrels, 1, Mathf.Min(row.BarrelsMax, context.Mounts.Length));
    }

    public virtual bool IsReady => _cooldownLeft <= 0f;

    public virtual float Readiness => _row.Rate <= 0f ? 1f : 1f - Mathf.Clamp01(_cooldownLeft / _row.Rate);

    protected WeaponRow Row => _row;

    protected WeaponContext Context => _context;

    protected int Barrels => _barrels;

    public void Fire()
    {
        if (IsReady == false)
            return;

        _cooldownLeft = _row.Rate;
        OnFire();
    }

    public void Tick(float deltaTime)
    {
        if (_cooldownLeft > 0f)
            _cooldownLeft -= deltaTime;

        OnTick(deltaTime);
    }

    protected Transform GetMount(int barrel)
    {
        return _context.Mounts[barrel % _context.Mounts.Length];
    }

    protected Quaternion GetRotation(int barrel)
    {
        float angle = FireSpread.GetAngle(_row.FirePattern, barrel, _barrels);

        return GetMount(barrel).rotation * Quaternion.Euler(0f, 0f, angle);
    }

    protected abstract void OnFire();

    protected virtual void OnTick(float deltaTime)
    {
    }
}
