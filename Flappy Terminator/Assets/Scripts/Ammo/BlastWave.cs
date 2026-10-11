using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BlastWave : MonoBehaviour, IPoolable
{
    private const float GrowShare = 0.6f;

    [SerializeField] private float _lifetime = 0.35f;

    private readonly Collider2D[] _touched = new Collider2D[16];
    private readonly HashSet<IDamageable> _damaged = new HashSet<IDamageable>();

    private SpriteRenderer _renderer;
    private ContactFilter2D _filter;
    private float _fromRadius;
    private float _toRadius;
    private float _timeLeft;
    private int _damage;

    public event Action<IPoolable> Released;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (_timeLeft <= 0f)
            return;

        _timeLeft -= Time.deltaTime;

        if (_timeLeft <= 0f)
        {
            Released?.Invoke(this);

            return;
        }

        Grow(1f - _timeLeft / _lifetime);
    }

    public void OnSpawn()
    {
        _timeLeft = 0f;
        _damaged.Clear();
    }

    public void OnDespawn()
    {
    }

    public void Play(float nearDiameter, float farDiameter, int damage, ContactFilter2D filter)
    {
        _fromRadius = nearDiameter * 0.5f;
        _toRadius = farDiameter * 0.5f;
        _damage = damage;
        _filter = filter;
        _timeLeft = _lifetime;
        _damaged.Clear();
        Grow(0f);
    }

    private void Grow(float life)
    {
        float radius = GetRadius(life);

        Show(life, radius);
        Damage(radius);
    }

    private void Show(float life, float radius)
    {
        float width = _renderer.sprite == null ? 1f : _renderer.sprite.bounds.size.x;
        float scale = radius * 2f / width;
        Color color = _renderer.color;

        color.a = 1f - life * life;
        _renderer.color = color;
        transform.localScale = new Vector3(scale, scale, 1f);
    }

    private float GetRadius(float life)
    {
        return Mathf.Lerp(_fromRadius, _toRadius, Mathf.Clamp01(life / GrowShare));
    }

    private void Damage(float radius)
    {
        if (_damage <= 0)
            return;

        int count = Physics2D.OverlapCircle(transform.position, radius, _filter, _touched);

        for (int i = 0; i < count; i++)
        {
            if (_touched[i].TryGetComponent(out IDamageable damageable) == false)
                damageable = _touched[i].GetComponentInParent<IDamageable>();

            if (damageable != null && damageable.IsVulnerable && _damaged.Add(damageable))
                damageable.TakeDamage(_damage);
        }
    }
}
