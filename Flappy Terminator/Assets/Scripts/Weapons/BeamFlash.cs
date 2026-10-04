using System;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BeamFlash : MonoBehaviour, IPoolable
{
    private const int SpiralPointsPerTurn = 16;
    private const int MaxSpiralPoints = 1024;
    private const float NearWidthShare = 1f;
    private const float FarWidthShare = 0.35f;

    [SerializeField] private float _lifetime = 0.08f;
    [SerializeField] private Sound _shotSound;
    [SerializeField] private Sound _loopSound;
    [SerializeField] private Sound _hitLoopSound;
    [SerializeField] private Sound _hitSound;
    [SerializeField] private Sound _worldHitSound;
    [SerializeField] private ExplosionAnimation _pierceEffect;
    [SerializeField] private AimLine _sight;
    [SerializeField] private LineRenderer _spiral;
    [SerializeField] private float _spiralRadius = 0.1f;
    [SerializeField] private float _spiralPitch = 0.3f;
    [SerializeField] private float _spiralLifetime = 0.28f;

    private LineRenderer _line;
    private GradientAlphaKey[] _alphaKeys;
    private GradientColorKey[] _colorKeys;
    private Gradient _gradient;
    private GradientColorKey[] _spiralColors;
    private GradientAlphaKey[] _spiralAlphas;
    private Gradient _spiralGradient;
    private float _age;

    public Sound ShotSound => _shotSound;

    public Sound LoopSound => _loopSound;

    public Sound HitLoopSound => _hitLoopSound;

    public Sound HitSound => _hitSound;

    public Sound WorldHitSound => _worldHitSound;

    public ExplosionAnimation PierceEffect => _pierceEffect;

    public AimLine Sight => _sight;

    public event Action<IPoolable> Released;

    private void Awake()
    {
        _line = GetComponent<LineRenderer>();
        _alphaKeys = _line.colorGradient.alphaKeys;
        _colorKeys = new GradientColorKey[3];
        _gradient = new Gradient();

        if (_spiral == null)
            return;

        _spiralColors = _spiral.colorGradient.colorKeys;
        _spiralAlphas = new GradientAlphaKey[2];
        _spiralGradient = new Gradient();
    }

    private void Update()
    {
        _age += Time.deltaTime;

        if (_age >= _lifetime)
            _line.enabled = false;

        if (_spiral != null)
            FadeSpiral();

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

        if (_spiral != null)
            Coil(from, to);
    }

    private float GetDuration()
    {
        return _spiral == null ? _lifetime : Mathf.Max(_lifetime, _spiralLifetime);
    }

    private void FadeSpiral()
    {
        float alpha = _spiralLifetime <= 0f ? 0f : 1f - Mathf.Clamp01(_age / _spiralLifetime);

        _spiralAlphas[0] = new GradientAlphaKey(alpha, 0f);
        _spiralAlphas[1] = new GradientAlphaKey(alpha, 1f);
        _spiralGradient.SetKeys(_spiralColors, _spiralAlphas);
        _spiral.colorGradient = _spiralGradient;
    }

    private void Coil(Vector3 from, Vector3 to)
    {
        Vector3 axis = to - from;
        float length = axis.magnitude;

        if (length < 0.001f || _spiralPitch <= 0f)
        {
            _spiral.positionCount = 0;

            return;
        }

        Vector3 direction = axis / length;
        Vector3 side = new Vector3(-direction.y, direction.x, 0f);
        int count = Mathf.Clamp(Mathf.CeilToInt(length / _spiralPitch * SpiralPointsPerTurn) + 1, 2, MaxSpiralPoints);
        Vector3[] points = new Vector3[count];
        Keyframe[] widths = new Keyframe[count];

        for (int i = 0; i < count; i++)
        {
            float along = (float)i / (count - 1);
            float phase = Mathf.PI * 2f * along * length / _spiralPitch;
            float depth = (Mathf.Cos(phase) + 1f) * 0.5f;

            points[i] = from + direction * (along * length) + side * (Mathf.Sin(phase) * _spiralRadius);
            widths[i] = new Keyframe(along, Mathf.Lerp(FarWidthShare, NearWidthShare, depth));
        }

        _spiral.useWorldSpace = true;
        _spiral.positionCount = count;
        _spiral.SetPositions(points);
        _spiral.widthCurve = new AnimationCurve(widths);
    }
}
