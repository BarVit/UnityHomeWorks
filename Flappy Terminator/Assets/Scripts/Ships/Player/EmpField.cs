using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EmpField : MonoBehaviour
{
    private const int BoltPoints = 7;
    private const int PlaceAttempts = 6;

    [SerializeField] private Transform _ship;
    [SerializeField] private SpriteRenderer _dome;
    [SerializeField] private EmpBolt _boltTemplate;
    [SerializeField] private ExplosionAnimation _popPrefab;
    [SerializeField] private LayerMask _enemyLayers;
    [SerializeField] private LayerMask _enemyAmmoLayers;
    [SerializeField] private float _growTime = 1f;
    [SerializeField] private float _radiusInScreens = 1f;
    [SerializeField] private float _fadeTime = 0.25f;
    [SerializeField] private float _minBoltDelay = 0.2f;
    [SerializeField] private float _maxBoltDelay = 0.5f;
    [SerializeField] private float _minBoltLength = 1.5f;
    [SerializeField] private float _maxBoltLength = 3.5f;
    [SerializeField] private float _boltJitter = 0.35f;

    private readonly Collider2D[] _hits = new Collider2D[64];
    private readonly HashSet<EnemyShip> _frozen = new HashSet<EnemyShip>();
    private readonly List<EmpBolt> _bolts = new List<EmpBolt>();
    private readonly Vector3[] _points = new Vector3[BoltPoints];

    private Camera _camera;
    private ContactFilter2D _enemyFilter;
    private ContactFilter2D _ammoFilter;
    private SpawnPool<ExplosionAnimation> _pops;
    private Color _domeColor;
    private float _duration;
    private float _elapsed;
    private float _boltLeft;

    public bool IsActive { get; private set; }

    public event Action Finished;

    private void Awake()
    {
        _camera = Camera.main;
        _enemyFilter = CreateFilter(_enemyLayers);
        _ammoFilter = CreateFilter(_enemyAmmoLayers);
        _pops = new PoolRegistry("EMP").GetPool(_popPrefab);
        _domeColor = _dome.color;
        _dome.enabled = false;
        _boltTemplate.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (IsActive == false)
            return;

        _elapsed += Time.deltaTime;

        if (_elapsed >= _duration)
        {
            Stop();
            Finished?.Invoke();

            return;
        }

        float radius = GetRadius();
        Vector2 center = _ship.position;

        Place(center, radius);
        FreezeEnemies(center, radius);
        PopAmmo(center, radius);
        Flash(center, radius, Time.deltaTime);
    }

    public void Launch(float duration)
    {
        _duration = Mathf.Max(0f, duration);
        _elapsed = 0f;
        _boltLeft = Random.Range(_minBoltDelay, _maxBoltDelay);
        IsActive = true;
        _dome.enabled = true;
        Place(_ship.position, 0f);
    }

    public void Stop()
    {
        IsActive = false;
        _dome.enabled = false;

        foreach (EnemyShip ship in _frozen)
        {
            if (ship != null)
                ship.Unfreeze();
        }

        _frozen.Clear();
    }

    private static ContactFilter2D CreateFilter(LayerMask layers)
    {
        ContactFilter2D filter = new ContactFilter2D { useTriggers = true };
        filter.SetLayerMask(layers);

        return filter;
    }

    private float GetRadius()
    {
        float full = (ScreenArea.GetRightEdge(_camera) - ScreenArea.GetLeftEdge(_camera)) * _radiusInScreens;

        return _growTime <= 0f ? full : full * Mathf.Clamp01(_elapsed / _growTime);
    }

    private void Place(Vector2 center, float radius)
    {
        Transform dome = _dome.transform;
        float spriteSize = _dome.sprite.bounds.size.x;
        Vector3 parentScale = dome.parent == null ? Vector3.one : dome.parent.lossyScale;

        dome.SetPositionAndRotation(center, Quaternion.identity);
        dome.localScale = new Vector3(2f * radius / (spriteSize * parentScale.x),
            2f * radius / (spriteSize * parentScale.y), 1f);

        float fade = _fadeTime <= 0f ? 1f : Mathf.Clamp01((_duration - _elapsed) / _fadeTime);
        _dome.color = new Color(_domeColor.r, _domeColor.g, _domeColor.b, _domeColor.a * fade);
    }

    private void FreezeEnemies(Vector2 center, float radius)
    {
        int count = Physics2D.OverlapCircle(center, radius, _enemyFilter, _hits);

        for (int i = 0; i < count; i++)
        {
            EnemyShip ship = _hits[i].GetComponentInParent<EnemyShip>();

            if (ship != null && ship.IsDead == false && _frozen.Add(ship))
                ship.Freeze();
        }
    }

    private void PopAmmo(Vector2 center, float radius)
    {
        int count = Physics2D.OverlapCircle(center, radius, _ammoFilter, _hits);

        for (int i = 0; i < count; i++)
        {
            Ammo ammo = _hits[i].GetComponentInParent<Ammo>();

            if (ammo == null || ammo.gameObject.activeInHierarchy == false)
                continue;

            _pops.Get(ammo.transform.position).Play();
            ammo.Remove();
        }
    }

    private void Flash(Vector2 center, float radius, float deltaTime)
    {
        _boltLeft -= deltaTime;

        if (_boltLeft > 0f || radius <= 0f)
            return;

        _boltLeft = Random.Range(_minBoltDelay, _maxBoltDelay);

        for (int attempt = 0; attempt < PlaceAttempts; attempt++)
        {
            Vector2 start = center + Random.insideUnitCircle * radius;

            if (ScreenArea.Contains(_camera, start) == false)
                continue;

            DrawBolt(start);

            return;
        }
    }

    private void DrawBolt(Vector2 start)
    {
        Vector2 direction = Random.insideUnitCircle.normalized;
        Vector2 side = new Vector2(-direction.y, direction.x);
        float length = Random.Range(_minBoltLength, _maxBoltLength);

        for (int i = 0; i < BoltPoints; i++)
        {
            float share = i / (float)(BoltPoints - 1);
            float jitter = i == 0 || i == BoltPoints - 1 ? 0f : Random.Range(-_boltJitter, _boltJitter);

            _points[i] = start + direction * (length * share) + side * jitter;
        }

        GetBolt().Show(_points, BoltPoints);
    }

    private EmpBolt GetBolt()
    {
        foreach (EmpBolt bolt in _bolts)
        {
            if (bolt.IsBusy == false)
                return bolt;
        }

        EmpBolt created = Instantiate(_boltTemplate, _boltTemplate.transform.parent);
        _bolts.Add(created);

        return created;
    }
}
