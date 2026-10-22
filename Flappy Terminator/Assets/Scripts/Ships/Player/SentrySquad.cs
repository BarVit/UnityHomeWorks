using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SentrySquad : MonoBehaviour
{
    private const int DroneCount = 2;
    private const float FacingRight = 270f;
    private const float SparkSpread = 60f;
    private const float HitReachMargin = 1f;

    [SerializeField] private Transform _ship;
    [SerializeField] private SpriteRenderer _body;
    [SerializeField] private Health _health;
    [SerializeField] private SpriteRenderer[] _drones = new SpriteRenderer[DroneCount];
    [SerializeField] private SentryTracer _tracerTemplate;
    [SerializeField] private ParticleSystem _sparks;
    [SerializeField] private LayerMask _targetLayers;
    [SerializeField, Range(0.1f, 1f)] private float _sizeShare = 0.5f;
    [SerializeField, Range(0f, 1f)] private float _gapShare = 0.2f;
    [SerializeField] private float _shotsPerSecond = 10f;
    [SerializeField] private int _sparksPerHit = 3;
    [SerializeField] private float _sparkSpeed = 2.5f;
    [SerializeField] private float _sparkSize = 0.05f;
    [SerializeField] private float _sparkLifetime = 0.15f;

    private readonly Collider2D[] _candidates = new Collider2D[32];
    private readonly RaycastHit2D[] _hits = new RaycastHit2D[8];
    private readonly List<SentryTracer> _tracers = new List<SentryTracer>();
    private readonly Vector2[] _offsets = new Vector2[DroneCount];
    private readonly float[] _owedDamage = new float[DroneCount];
    private readonly float[] _shotLeft = new float[DroneCount];

    private Camera _camera;
    private ContactFilter2D _filter;
    private SentrySpec _spec;
    private float _lifeLeft;
    private float _attackLeft;
    private float _reloadLeft;
    private float _noseOffset;

    public bool IsActive { get; private set; }

    public event Action Finished;

    private void Awake()
    {
        _camera = Camera.main;
        _filter = new ContactFilter2D { useTriggers = true };
        _filter.SetLayerMask(_targetLayers);
        _tracerTemplate.gameObject.SetActive(false);
        Show(false);
    }

    private void Update()
    {
        if (IsActive == false)
            return;

        if (_health.Value == 0)
        {
            Stop();

            return;
        }

        _lifeLeft -= Time.deltaTime;

        if (_lifeLeft <= 0f)
        {
            Stop();
            Finished?.Invoke();

            return;
        }

        Fire(Time.deltaTime);
    }

    private void LateUpdate()
    {
        if (IsActive)
            Follow();
    }

    public void Launch(SentrySpec spec)
    {
        _spec = spec;
        _lifeLeft = spec.Lifetime;
        _attackLeft = spec.AttackTime;
        _reloadLeft = 0f;

        for (int i = 0; i < DroneCount; i++)
        {
            _owedDamage[i] = 0f;
            _shotLeft[i] = i / (_shotsPerSecond * DroneCount);
        }

        Measure();
        IsActive = true;
        Follow();
        Show(true);
    }

    public void Stop()
    {
        IsActive = false;
        Show(false);
    }

    private void Fire(float deltaTime)
    {
        if (_reloadLeft > 0f)
        {
            _reloadLeft -= deltaTime;

            if (_reloadLeft <= 0f)
                _attackLeft = _spec.AttackTime;

            return;
        }

        EnemyShip target = FindTarget();

        if (target == null)
            return;

        float interval = 1f / _shotsPerSecond;

        _attackLeft -= deltaTime;

        for (int i = 0; i < DroneCount; i++)
        {
            _shotLeft[i] -= deltaTime;

            while (_shotLeft[i] <= 0f && target.IsDead == false)
            {
                Shoot(i, target, interval);
                _shotLeft[i] += interval;
            }
        }

        if (_attackLeft <= 0f)
            _reloadLeft = Mathf.Max(Mathf.Epsilon, _spec.Reload);
    }

    private void Shoot(int drone, EnemyShip target, float interval)
    {
        Vector2 muzzle = GetMuzzle(drone);
        Vector2 point = GetHitPoint(muzzle, target);

        _owedDamage[drone] += _spec.DamagePerSecond * interval;

        int damage = Mathf.FloorToInt(_owedDamage[drone]);

        if (damage > 0)
        {
            _owedDamage[drone] -= damage;
            DamageLog.SetSource(DamageLog.Sentry + "#" + drone);
            target.TakeDamage(damage);
        }

        GetTracer().Show(muzzle, point);
        Spark(point, muzzle);
    }

    private EnemyShip FindTarget()
    {
        Vector2 center = _camera.transform.position;
        Vector2 half = new Vector2(_camera.orthographicSize * _camera.aspect, _camera.orthographicSize);
        int count = Physics2D.OverlapArea(center - half, center + half, _filter, _candidates);
        EnemyShip best = null;

        for (int i = 0; i < count; i++)
        {
            EnemyShip ship = _candidates[i].GetComponentInParent<EnemyShip>();

            if (ship == null || ship == best || ship.IsDead || ship.IsVulnerable == false)
                continue;

            if (ScreenArea.Contains(_camera, ship.transform.position) == false)
                continue;

            if (best == null || ship.CurrentHealth > best.CurrentHealth)
                best = ship;
        }

        return best;
    }

    private Vector2 GetHitPoint(Vector2 muzzle, EnemyShip target)
    {
        Vector2 toTarget = (Vector2)target.transform.position - muzzle;
        int count = Physics2D.Raycast(muzzle, toTarget.normalized, _filter, _hits, toTarget.magnitude + HitReachMargin);

        for (int i = 0; i < count; i++)
        {
            if (ReferenceEquals(WeaponDelivery.FindDamageable(_hits[i].collider), target))
                return _hits[i].point;
        }

        return target.transform.position;
    }

    private void Spark(Vector2 point, Vector2 from)
    {
        Vector2 back = (from - point).normalized;

        for (int i = 0; i < _sparksPerHit; i++)
        {
            Vector2 direction = Quaternion.Euler(0f, 0f, Random.Range(-SparkSpread, SparkSpread)) * back;

            ParticleSystem.EmitParams spark = new ParticleSystem.EmitParams
            {
                position = point,
                velocity = direction * (_sparkSpeed * Random.Range(0.6f, 1f)),
                startSize = _sparkSize,
                startLifetime = _sparkLifetime,
                applyShapeToPosition = false
            };

            _sparks.Emit(spark, 1);
        }
    }

    private SentryTracer GetTracer()
    {
        foreach (SentryTracer tracer in _tracers)
        {
            if (tracer.IsBusy == false)
                return tracer;
        }

        SentryTracer created = Instantiate(_tracerTemplate, _tracerTemplate.transform.parent);
        _tracers.Add(created);

        return created;
    }

    private void Measure()
    {
        Bounds ship = GetShipBounds();
        Bounds sprite = _drones[0].sprite.bounds;
        float scale = _sizeShare * Mathf.Max(ship.size.x, ship.size.y) / Mathf.Max(sprite.size.x, sprite.size.y);
        float length = sprite.size.y * scale;
        float height = sprite.size.x * scale;
        float gap = _gapShare * ship.size.y;
        float centerX = ship.max.x - length * 0.5f;

        _noseOffset = length * 0.5f;
        _offsets[0] = new Vector2(centerX, ship.max.y + gap + height * 0.5f);
        _offsets[1] = new Vector2(centerX, ship.min.y - gap - height * 0.5f);

        foreach (SpriteRenderer drone in _drones)
            drone.transform.localScale = Vector3.one * scale;
    }

    private Bounds GetShipBounds()
    {
        Matrix4x4 toShip = _ship.worldToLocalMatrix * _body.transform.localToWorldMatrix;
        Bounds sprite = _body.sprite.bounds;
        Bounds result = new Bounds(toShip.MultiplyPoint3x4(sprite.min), Vector3.zero);

        result.Encapsulate(toShip.MultiplyPoint3x4(sprite.max));
        result.Encapsulate(toShip.MultiplyPoint3x4(new Vector3(sprite.min.x, sprite.max.y)));
        result.Encapsulate(toShip.MultiplyPoint3x4(new Vector3(sprite.max.x, sprite.min.y)));

        return result;
    }

    private void Follow()
    {
        Vector2 origin = _ship.position;
        Quaternion facing = Quaternion.Euler(0f, 0f, FacingRight);

        for (int i = 0; i < DroneCount; i++)
            _drones[i].transform.SetPositionAndRotation(origin + _offsets[i], facing);
    }

    private Vector2 GetMuzzle(int drone)
    {
        return (Vector2)_drones[drone].transform.position + Vector2.right * _noseOffset;
    }

    private void Show(bool isShown)
    {
        foreach (SpriteRenderer drone in _drones)
        {
            if (drone != null)
                drone.enabled = isShown;
        }
    }
}
