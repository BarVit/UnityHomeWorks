using UnityEngine;

public abstract class WeaponDelivery
{
    private const float MaxHeat = 100f;

    private readonly WeaponRow _row;
    private readonly WeaponContext _context;
    private readonly int _barrels;
    private readonly int[] _ranks;

    private readonly SpawnPool<ExplosionAnimation> _shotPool;

    private float _cooldownLeft;
    private float _heat;
    private bool _isOverheated;
    private int _shotsLeft;
    private float _reloadLeft;

    protected WeaponDelivery(WeaponRow row, WeaponContext context)
    {
        _row = row;
        _context = context;
        _barrels = Mathf.Clamp(context.Barrels, 1, Mathf.Min(row.BarrelsMax, context.Mounts.Length));
        _ranks = RankByHeight();
        _shotsLeft = row.Magazine;

        if (row.Assets != null && row.Assets.ShotEffect != null)
            _shotPool = context.Pools.GetPool(row.Assets.ShotEffect);
    }

    public virtual bool IsReady => _cooldownLeft <= 0f && _isOverheated == false && _reloadLeft <= 0f;

    public virtual Transform LockTarget => null;

    public bool IsOverheated => _isOverheated;

    public bool HasHeat => _row.HeatPerShot > 0f;

    public bool HasMagazine => _row.Magazine > 0;

    public int ShotsLeft => _shotsLeft;

    public int MagazineSize => _row.Magazine;

    public virtual float Readiness => _reloadLeft > 0f
        ? 1f - Mathf.Clamp01(_reloadLeft / _row.ReloadTime)
        : (HasHeat
            ? 1f - Mathf.Clamp01(_heat / MaxHeat)
            : (_row.Rate <= 0f ? 1f : 1f - Mathf.Clamp01(_cooldownLeft / _row.Rate)));

    protected WeaponRow Row => _row;

    protected WeaponContext Context => _context;

    protected int Barrels => _barrels;

    protected float HeatShare => Mathf.Clamp01(_heat / MaxHeat);

    protected float ProjectileSpeed => _row.ProjectileSpeed;

    protected int Damage => _context.Damage > 0 ? _context.Damage : _row.Damage;

    public void Fire()
    {
        if (IsReady == false)
            return;

        _cooldownLeft = _row.Rate;
        Warm();
        OnFire();
        Spend();
    }

    public void Tick(float deltaTime)
    {
        if (_cooldownLeft > 0f)
            _cooldownLeft -= deltaTime;

        Cool(deltaTime);
        Reload(deltaTime);
        OnTick(deltaTime);
    }

    protected Transform GetMount(int barrel)
    {
        return _context.Mounts[barrel % _context.Mounts.Length];
    }

    protected Quaternion GetRotation(int barrel)
    {
        float angle = FireSpread.GetAngle(_row.FirePattern, _ranks[barrel], _barrels, _row.SideAngle);

        return GetMount(barrel).rotation * Quaternion.Euler(0f, 0f, angle);
    }

    protected void ShowShot(int barrel)
    {
        if (_shotPool == null)
            return;

        ExplosionAnimation shot = _shotPool.Get(GetMount(barrel).position, GetRotation(barrel));

        shot.Play();
    }

    private int[] RankByHeight()
    {
        int[] ranks = new int[_barrels];
        Vector3 up = GetMount(0).up;

        for (int barrel = 0; barrel < _barrels; barrel++)
            for (int other = 0; other < _barrels; other++)
                if (IsBelow(other, barrel, up))
                    ranks[barrel]++;

        return ranks;
    }

    private bool IsBelow(int other, int barrel, Vector3 up)
    {
        float otherHeight = Vector3.Dot(GetMount(other).position, up);
        float height = Vector3.Dot(GetMount(barrel).position, up);

        return otherHeight < height || (Mathf.Approximately(otherHeight, height) && other < barrel);
    }

    private void Spend()
    {
        if (HasMagazine == false)
            return;

        _shotsLeft--;

        if (_shotsLeft > 0)
            return;

        _reloadLeft = _row.ReloadTime;

        if (_reloadLeft <= 0f)
            _shotsLeft = _row.Magazine;
    }

    private void Reload(float deltaTime)
    {
        if (_reloadLeft <= 0f)
            return;

        _reloadLeft -= deltaTime;

        if (_reloadLeft <= 0f)
            _shotsLeft = _row.Magazine;
    }

    private void Warm()
    {
        if (HasHeat == false)
            return;

        _heat += _row.HeatPerShot * _barrels;

        if (_heat < MaxHeat)
            return;

        _heat = MaxHeat;
        _isOverheated = true;
    }

    private void Cool(float deltaTime)
    {
        if (_heat <= 0f)
            return;

        _heat = Mathf.Max(0f, _heat - _row.CooldownRate * deltaTime);

        if (_heat <= MaxHeat * _row.UnlockHeat)
            _isOverheated = false;
    }

    public virtual void Dispose()
    {
    }

    protected abstract void OnFire();

    protected virtual void OnTick(float deltaTime)
    {
    }
}
