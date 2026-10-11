using System;
using UnityEngine;

[RequireComponent(typeof(Health), typeof(EnemyMover), typeof(EnemyShooter))]
public class EnemyShip : MonoBehaviour, IPoolable, IRemoveable, IDamageable
{
    private const float VisiblePixels = 2f;

    [SerializeField] private AmmoHitHandler _ammoHitHandler;
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

    public int BodyDamage { get; private set; }

    public string ShipId { get; private set; }

    public int Tier { get; private set; }

    public Reward Reward { get; private set; }

    public EnemySetup Setup { get; private set; }

    public bool IsDead => _isDead;

    public int Phase => _phases == null ? BossPhaseRunner.NoPhase : _phases.Current;

    public event Action<IPoolable> Released;
    public event Action<EnemyShip> Died;
    public event Action<EnemyShip> Retired;

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
    }

    private void OnDisable()
    {
        _ammoHitHandler.HitDetected -= OnAmmoHit;
        _health.Died -= Die;
    }

    private void LateUpdate()
    {
        _skin.Apply();
    }

    private void Update()
    {
        _phases?.Tick(Time.deltaTime);

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
        Arm(setup);
        _mover.Fly(setup.Movement);
        _shooter.StartShoot();
    }

    public void OnSpawn()
    {
        _isDead = false;
    }

    public void OnDespawn()
    {
        _phases?.Stop();
        _phases = null;
        _shooter.StopShoot();
        _mover.Stop();
        Retired?.Invoke(this);
    }

    public void Remove()
    {
        Released?.Invoke(this);
    }

    public void TakeDamage(int damage)
    {
        if (IsShown() == false)
            return;

        _health.TakeDamage(damage);
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

    private void Arm(EnemySetup setup)
    {
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
        TakeDamage(ammo.Damage);
    }

    private bool IsShown()
    {
        return _body == null || ScreenArea.Shows(_camera, _body.bounds, VisiblePixels);
    }
}
