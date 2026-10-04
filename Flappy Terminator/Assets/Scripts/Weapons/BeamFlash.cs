using System;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BeamFlash : MonoBehaviour, IPoolable
{
    [SerializeField] private float _lifetime = 0.08f;
    [SerializeField] private Sound _shotSound;
    [SerializeField] private Sound _loopSound;
    [SerializeField] private Sound _hitLoopSound;
    [SerializeField] private Sound _hitSound;
    [SerializeField] private Sound _worldHitSound;
    [SerializeField] private ExplosionAnimation _pierceEffect;
    [SerializeField] private AimLine _sight;
    [SerializeField] private BeamSpiral[] _spirals;
    [SerializeField] private float _spiralLifetime = 0.28f;

    private LineRenderer _line;
    private GradientAlphaKey[] _alphaKeys;
    private GradientColorKey[] _colorKeys;
    private Gradient _gradient;
    private float _age;

    public Sound ShotSound => _shotSound;

    public Sound LoopSound => _loopSound;

    public Sound HitLoopSound => _hitLoopSound;

    public Sound HitSound => _hitSound;

    public Sound WorldHitSound => _worldHitSound;

    public ExplosionAnimation PierceEffect => _pierceEffect;

    public AimLine Sight => _sight;

    private bool HasSpirals => _spirals != null && _spirals.Length > 0;

    public event Action<IPoolable> Released;

    private void Awake()
    {
        _line = GetComponent<LineRenderer>();
        _alphaKeys = _line.colorGradient.alphaKeys;
        _colorKeys = new GradientColorKey[3];
        _gradient = new Gradient();
    }

    private void Update()
    {
        _age += Time.deltaTime;

        if (_age >= _lifetime)
            _line.enabled = false;

        FadeSpirals();

        if (_age < GetDuration())
            return;

        Released?.Invoke(this);
    }

    public void OnSpawn()
    {
        _age = 0f;
        _line.enabled = true;
    }

    public void OnDespawn()
    {
    }

    public void Tint(Color color)
    {
        _colorKeys[0] = new GradientColorKey(Color.Lerp(color, Color.white, 0.7f), 0f);
        _colorKeys[1] = new GradientColorKey(color, 0.25f);
        _colorKeys[2] = new GradientColorKey(color, 1f);

        _gradient.SetKeys(_colorKeys, _alphaKeys);
        _line.colorGradient = _gradient;
    }

    public void Show(Vector3 from, Vector3 to)
    {
        _line.useWorldSpace = true;
        _line.positionCount = 2;
        _line.SetPosition(0, from);
        _line.SetPosition(1, to);
        _line.enabled = true;
        _age = 0f;

        if (_spirals == null)
            return;

        foreach (BeamSpiral spiral in _spirals)
            spiral.Show(from, to);

        FadeSpirals();
    }

    private float GetDuration()
    {
        return HasSpirals ? Mathf.Max(_lifetime, _spiralLifetime) : _lifetime;
    }

    private void FadeSpirals()
    {
        if (HasSpirals == false)
            return;

        float share = _spiralLifetime <= 0f ? 0f : 1f - Mathf.Clamp01(_age / _spiralLifetime);

        foreach (BeamSpiral spiral in _spirals)
            spiral.Fade(share);
    }
}
