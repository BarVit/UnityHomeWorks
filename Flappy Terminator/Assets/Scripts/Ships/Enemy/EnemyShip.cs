using System;
using UnityEngine;

[RequireComponent(typeof(Health), typeof(EnemyMover), typeof(EnemyShooter))]
public class EnemyShip : MonoBehaviour, IPoolable, IRemoveable, IDamageable
{
    private const float VisiblePixels = 2f;
    private const int StasisSortingShift = 2;
    private const float StasisSizeShare = 1.3f;
    private const float StasisPulseRate = 3f;
    private const float StasisMinAlpha = 0.3f;

    private static readonly Color StasisColor = new Color(0.7f, 0.45f, 1f, 0.75f);

    [SerializeField] private AmmoHitHandler _ammoHitHandler;
    [SerializeField] private EnergyShield _shield;
    [SerializeField] private AmmoHitHandler _shieldHitHandler;
    [SerializeField] private float _exitMargin = 1f;

    private Camera _camera;
    private Animator _animator;
    private SpriteRenderer _body;
    private SkinnedSprite _skin;
    private SpriteSkin _skinAsset;
    private Health _health;
    private EnemyMover _mover;
    private EnemyShooter _shooter;
    private EnemyContext _context;
    private BossPhaseRunner _phases;
    private bool _isDead;
    private bool _isEmpFrozen;
    private float _stasisLeft;
    private SpriteRenderer _stasisField;
    private bool _isHoldingFire;

    public int BodyDamage { get; private set; }

    public string ShipId { get; private set; }

    public int Tier { get; private set; }

    public Reward Reward { get; private set; }

    public EnemySetup Setup { get; private set; }

    public bool IsDead => _isDead;

    public int CurrentHealth => _health.Value;

    public int MaxHealth => _health.MaxValue;

    public bool IsFrozen { get; private set; }

    public string LogName => ShipId == null ? name : ShipId.Replace("enemy_", string.Empty) + Tier;

    public bool IsVulnerable => IsShown();

    public int Phase => _phases == null ? BossPhaseRunner.NoPhase : _phases.Current;

    public event Action<IPoolable> Released;
    public event Action<EnemyShip> Died;
    public event Action<EnemyShip> Retired;
    public event Action<EnemyShip> ShieldBroken;

    private void Awake()
    {
        _camera = Camera.main;
        _animator = GetComponentInChildren<Animator>(true);
        _body = GetComponentInChildren<SpriteRenderer>(true);
        _skin = new SkinnedSprite(_body);
        _health = GetComponent<Health>();
        _mover = GetComponent<EnemyMover>();
        _shooter = GetComponent<EnemyShooter>();
    }

    private void OnEnable()
    {
        _ammoHitHandler.HitDetected += OnAmmoHit;
        _health.Died += Die;

        if (_shieldHitHandler != null)
            _shieldHitHandler.HitDetected += OnAmmoHit;

        if (_shield != null)
            _shield.Broken += OnShieldBroken;
    }

    private void OnDisable()
    {
        _ammoHitHandler.HitDetected -= OnAmmoHit;
        _health.Died -= Die;

        if (_shieldHitHandler != null)
            _shieldHitHandler.HitDetected -= OnAmmoHit;

        if (_shield != null)
            _shield.Broken -= OnShieldBroken;
    }

    private void LateUpdate()
    {
        _skin.Apply();
    }

    private void Update()
    {
        TickStasis(Time.deltaTime);

        if (IsFrozen)
            return;

        _phases?.Tick(Time.deltaTime);

        if (_isHoldingFire && HasEntered())
        {
            _isHoldingFire = false;
            _shooter.StartShoot();
        }

        if (transform.position.x + _exitMargin < ScreenArea.GetLeftEdge(_camera))
            Remove();
    }

    public void Init(EnemyContext context)
    {
        _context = context;
    }

    public void Launch(EnemySetup setup)
    {
        Setup = setup;
        ShipId = setup.ShipId;
        Tier = setup.Tier;
        BodyDamage = setup.BodyDamage;
        Reward = new Reward(setup.RewardXp, setup.RewardMoney);

        transform.localScale = Vector3.one * setup.Scale;

        if (_body != null)
            _body.color = setup.Tint;

        _skinAsset = setup.Skin;
        _skin.Wear(_skinAsset);

        _health.Init(setup.Health);
        RaiseShield(setup.ShieldCapacity, setup.ShieldLevel);
        Arm(setup);
        _mover.Fly(setup.Movement);
        _isHoldingFire = true;
    }

    public void OnSpawn()
    {
        _isDead = false;
    }

    public void Freeze()
    {
        if (_isDead)
            return;

        _isEmpFrozen = true;
        ApplyFreeze();
    }

    public void Unfreeze()
    {
        _isEmpFrozen = false;
        ApplyFreeze();
    }

    public void Hold(float seconds)
    {
        if (_isDead || seconds <= 0f)
            return;

        _stasisLeft = Mathf.Max(_stasisLeft, seconds);
        ApplyFreeze();
    }

    public void OnDespawn()
    {
        _isEmpFrozen = false;
        _stasisLeft = 0f;
        ApplyFreeze();
        _phases?.Stop();
        _phases = null;
        _shooter.StopShoot();
        _mover.Stop();

        if (_shield != null)
            _shield.Drop();

        Retired?.Invoke(this);
    }

    public void Remove()
    {
        Released?.Invoke(this);
    }

    public void TakeDamage(int damage)
    {
        string source = DamageLog.TakeSource();
        int before = _health.Value;

        if (IsVulnerable == false)
        {
            DamageLog.Hit("ignored", source, LogName, GetInstanceID(), damage, 0, 0, before, before, _health.MaxValue,
                "not on screen");

            return;
        }

        int dealt = damage;

        if (_shield != null)
            dealt = _shield.Absorb(damage);

        if (dealt > 0)
            _health.TakeDamage(dealt);

        DamageLog.Hit(_health.Value == 0 && before > 0 ? "kill" : "hit", source, LogName, GetInstanceID(), damage,
            damage - Mathf.Max(0, dealt), before - _health.Value, before, _health.Value, _health.MaxValue);
    }

    public void Die()
    {
        if (_isDead)
            return;

        _isDead = true;

        EnemyDeadBody deadBody = _context.DeadBodies.Get(transform.position);

        deadBody.transform.localScale = transform.localScale;
        deadBody.End(_animator == null ? null : _animator.runtimeAnimatorController, _skinAsset);

        ExplosionAnimation explosion = _context.Explosions.Get(transform.position);

        explosion.transform.localScale = transform.localScale;
        explosion.Play();

        Released?.Invoke(this);
        Died?.Invoke(this);
    }

    private void TickStasis(float deltaTime)
    {
        if (_stasisLeft <= 0f)
            return;

        _stasisLeft -= deltaTime;

        if (_stasisLeft <= 0f)
        {
            _stasisLeft = 0f;
            ApplyFreeze();

            return;
        }

        PulseStasis();
    }

    private void ApplyFreeze()
    {
        bool isFrozen = _isEmpFrozen || _stasisLeft > 0f;

        ShowStasis(_stasisLeft > 0f);

        if (isFrozen == IsFrozen)
            return;

        IsFrozen = isFrozen;

        if (isFrozen)
            _mover.Freeze();
        else
            _mover.Unfreeze();

        _shooter.Freeze(isFrozen);

        if (_animator != null)
            _animator.speed = isFrozen ? 0f : 1f;
    }

    private void ShowStasis(bool isShown)
    {
        if (isShown && _stasisField == null)
            _stasisField = CreateStasisField();

        if (_stasisField == null)
            return;

        _stasisField.enabled = isShown;

        if (isShown)
            PulseStasis();
    }

    private SpriteRenderer CreateStasisField()
    {
        if (_context == null || _context.StasisSprite == null || _body == null)
            return null;

        GameObject field = new GameObject("Stasis Field");
        field.transform.SetParent(transform, false);

        SpriteRenderer sprite = field.AddComponent<SpriteRenderer>();
        sprite.sprite = _context.StasisSprite;
        sprite.sortingLayerID = _body.sortingLayerID;
        sprite.sortingOrder = _body.sortingOrder + StasisSortingShift;

        return sprite;
    }

    private void PulseStasis()
    {
        if (_stasisField == null || _body == null)
            return;

        Bounds body = _body.bounds;
        float size = Mathf.Max(body.size.x, body.size.y) * StasisSizeShare;
        float spriteSize = Mathf.Max(0.0001f, _stasisField.sprite.bounds.size.x * transform.lossyScale.x);
        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * StasisPulseRate * Mathf.PI * 2f);

        _stasisField.transform.position = body.center;
        _stasisField.transform.localScale = Vector3.one * (size / spriteSize);
        _stasisField.color = new Color(StasisColor.r, StasisColor.g, StasisColor.b,
            Mathf.Lerp(StasisMinAlpha, StasisColor.a, pulse));
    }

    private void OnShieldBroken()
    {
        ShieldBroken?.Invoke(this);
    }

    private void RaiseShield(int capacity, int level)
    {
        if (_shield == null)
            return;

        if (capacity > 0)
            _shield.Raise(capacity, level);
        else
            _shield.Drop();
    }

    private void Arm(EnemySetup setup)
    {
        _shooter.ArmSpecial(setup.Special, setup.SpecialEvery, _context);

        if (setup.Phases.Count == 0)
        {
            _shooter.Arm(setup.Armament, _context);

            return;
        }

        _phases = new BossPhaseRunner(setup.Phases, _health, _shooter, _context);
        _phases.Start();
    }

    private void OnAmmoHit(Ammo ammo)
    {
        DamageLog.SetSource(ammo.Source + (ammo.IsCrit ? " crit" : string.Empty));
        TakeDamage(ammo.Damage);
    }

    private bool HasEntered()
    {
        Vector3 centre = _body == null ? transform.position : _body.bounds.center;

        return ScreenArea.Contains(_camera, centre);
    }

    private bool IsShown()
    {
        return _body == null || ScreenArea.Shows(_camera, _body.bounds, VisiblePixels);
    }
}
