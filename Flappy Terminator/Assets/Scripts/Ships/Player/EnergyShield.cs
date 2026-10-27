using System;
using UnityEngine;

public class EnergyShield : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _bubble;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private float _raiseDuration = 0.2f;
    [SerializeField] private float _breakDuration = 0.5f;
    [SerializeField] private float _flashDuration = 0.15f;
    [SerializeField] private float _pulseSpeed = 3f;
    [SerializeField] private float _pulseAmount = 0.04f;
    [SerializeField] private float _minAlpha = 0.35f;
    [SerializeField] private float _maxAlpha = 0.9f;
    [SerializeField] private float _breakScale = 1.6f;

    private Vector3 _baseScale;
    private Color _color;
    private int _capacity;
    private int _left;
    private float _shownFor;
    private float _flashLeft;
    private float _breakLeft;

    public bool IsUp => _left > 0;

    public bool IsGuarding => IsUp || _breakLeft > 0f;

    public event Action Broken;

    private void Awake()
    {
        _baseScale = _bubble.transform.localScale;
        _bubble.enabled = false;
        _collider.enabled = false;
    }

    private void Update()
    {
        if (IsUp)
            ShowUp();
        else if (_breakLeft > 0f)
            ShowBreak();
    }

    public void Raise(int capacity, int level)
    {
        Raise(capacity, ShieldColors.Get(level));
    }

    public void Raise(int capacity, Color color)
    {
        _color = color;
        _capacity = Mathf.Max(1, capacity);
        _left = _capacity;
        _shownFor = 0f;
        _flashLeft = 0f;
        _breakLeft = 0f;
        _bubble.enabled = true;
        _collider.enabled = true;
    }

    public int Absorb(int damage)
    {
        if (IsGuarding == false || damage <= 0)
            return damage;

        if (IsUp == false)
            return 0;

        _left = Mathf.Max(0, _left - damage);
        _flashLeft = _flashDuration;

        if (_left == 0)
            Break();

        return 0;
    }

    public void Drop()
    {
        _left = 0;
        _breakLeft = 0f;
        _bubble.enabled = false;
        _collider.enabled = false;
    }

    private void Break()
    {
        _breakLeft = _breakDuration;
        Broken?.Invoke();
    }

    private void ShowUp()
    {
        _shownFor += Time.deltaTime;
        _flashLeft = Mathf.Max(0f, _flashLeft - Time.deltaTime);

        float grow = Mathf.Clamp01(_shownFor / _raiseDuration);
        float pulse = 1f + Mathf.Sin(_shownFor * _pulseSpeed * Mathf.PI * 2f) * _pulseAmount;
        float share = (float)_left / _capacity;
        float alpha = Mathf.Lerp(_minAlpha, _maxAlpha, share);

        if (_flashLeft > 0f)
            alpha = 1f;

        _bubble.transform.localScale = _baseScale * (Mathf.SmoothStep(0.3f, 1f, grow) * pulse);
        _bubble.color = new Color(_color.r, _color.g, _color.b, alpha * grow);
    }

    private void ShowBreak()
    {
        _breakLeft -= Time.deltaTime;

        float progress = 1f - Mathf.Clamp01(_breakLeft / _breakDuration);

        _bubble.transform.localScale = _baseScale * Mathf.Lerp(1f, _breakScale, progress);
        _bubble.color = new Color(1f, 1f, 1f, 1f - progress);

        if (_breakLeft > 0f)
            return;

        _bubble.enabled = false;
        _collider.enabled = false;
    }
}
