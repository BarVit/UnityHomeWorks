using UnityEngine;

public abstract class WeaponDelivery
{
    private const float MaxHeat = 100f;
    private const float CritMarkInterval = 0.25f;

    private readonly WeaponRow _row;
    private readonly WeaponContext _context;
    private readonly int _barrels;
    private readonly int[] _ranks;

    private readonly SpawnPool<ExplosionAnimation> _shotPool;
    private readonly SpawnPool<CritMark> _critPool;

    private float _cooldownLeft;
    private float _heat;
    private bool _isOverheated;
    private int _shotsLeft;
    private float _reloadLeft;
    private float _critMarkUntil = -1f;

    protected WeaponDelivery(WeaponRow row, WeaponContext context)
    {
        _row = row;
        _context = context;
        _barrels = Mathf.Clamp(context.Barrels, 1, Mathf.Min(row.BarrelsMax, context.Mounts.Length));
        _ranks = RankByHeight();
        _shotsLeft = row.Magazine;

        if (row.Assets != null && row.Assets.ShotEffect != null)
            _shotPool = context.Pools.GetPool(row.Assets.ShotEffect);

        if (context.CritMark != null)
            _critPool = context.Pools.GetPool(context.CritMark);
    }

    public virtual bool IsReady => _cooldownLeft <= 0f && _isOverheated == false && _reloadLeft <= 0f;

    public virtual Transform LockTarget => null;

    public bool IsOverheated => _isOverheated;

    public float CooldownLeft => Mathf.Max(0f, _cooldownLeft);

    public float NextShotIn => Mathf.Max(CooldownLeft, _reloadLeft);

    public bool HasHeat => _row.HeatPerShot > 0f;

    public bool HasMagazine => _row.Magazine > 0;

    public int ShotsLeft => _shotsLeft;

    public int MagazineSize => _row.Magazine;

    public virtual float Readiness => _reloadLeft > 0f
        ? 1f - Mathf.Clamp01(_reloadLeft / _row.ReloadTime)
        : (HasHeat
            ? 1f - Mathf.Clamp01(_heat / MaxHeat)
            : (Cooldown <= 0f ? 1f : 1f - Mathf.Clamp01(_cooldownLeft / Cooldown)));

    protected WeaponRow Row => _row;

    protected WeaponContext Context => _context;

    protected int Barrels => _barrels;

    protected float HeatShare => Mathf.Clamp01(_heat / MaxHeat);

    protected float ProjectileSpeed => _row.ProjectileSpeed;

    private float Cooldown => _row.Rate * _context.RateScale;

    private int BaseDamage => _context.Damage > 0 ? _context.Damage : _row.Damage;

    protected static IDamageable FindDamageable(Collider2D collider)
    {
        if (collider.TryGetComponent(out IDamageable damageable) == false)
            damageable = collider.GetComponentInParent<IDamageable>();

        return damageable;
    }

    protected static bool IsOpenTarget(Collider2D collider)
    {
        IDamageable damageable = FindDamageable(collider);

        return damageable == null || damageable.IsVulnerable;
    }

    protected int GetDamage(int barrel)
    {
        return BaseDamage + _context.GetBoost(barrel).DamageBonus;
    }

    protected int RollDamage(int barrel, out bool isCrit)
    {
        WeaponBoost boost = _context.GetBoost(barrel);
        int damage = GetDamage(barrel);

        isCrit = boost.RollCrit();

        return isCrit ? boost.ApplyCrit(damage) : damage;
    }

    protected float RollDamage(float damage, int barrel, out bool isCrit)
    {
        WeaponBoost boost = _context.GetBoost(barrel);

        isCrit = boost.RollCrit();

        return isCrit ? boost.ApplyCrit(damage) : damage;
    }

    protected void CountFired()
    {
        _context.Tally?.CountFired();
    }

    protected void CountHit()
    {
        _context.Tally?.CountHit();
    }

    protected void ShowCrit(Vector2 point, int damage)
    {
        if (_critPool != null)
            _critPool.Get(point).Show(damage);
    }

    protected void ShowCrit(Vector2 point)
    {
        if (_critPool == null || Time.time < _critMarkUntil)
            return;

        _critMarkUntil = Time.time + CritMarkInterval;
        _critPool.Get(point).Show();
    }

    public bool Fire()
    {
        if (IsReady == false)
            return false;

        _cooldownLeft = Cooldown;
        Warm();
        OnFire();
        Spend();

        return true;
    }

    public bool IsAttackStart => HasMagazine == false || _shotsLeft == _row.Magazine;

    public int PassAttack()
    {
        if (IsReady == false)
            return 0;

        int shots = HasMagazine ? _shotsLeft : 1;

        _cooldownLeft = Cooldown;
        Warm();

        for (int shot = 0; shot < shots; shot++)
            Spend();

        return shots;
    }

    public void FireOutOfTurn()
    {
        OnFire();
    }

    public void ContinueCooldown(float cooldownLeft)
    {
        _cooldownLeft = Mathf.Max(_cooldownLeft, cooldownLeft);
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

        for (int barrel = 0; barrel < _barrels; barrel++)
            _heat += _row.HeatPerShot * _context.GetBoost(barrel).HeatScale;

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
