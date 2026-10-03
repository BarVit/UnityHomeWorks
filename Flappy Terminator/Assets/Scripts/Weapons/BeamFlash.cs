using System;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BeamFlash : MonoBehaviour, IPoolable
{
    [SerializeField] private float _lifetime = 0.08f;
    [SerializeField] private Sound _shotSound;
    [SerializeField] private Sound _hitSound;
    [SerializeField] private Sound _worldHitSound;

    private LineRenderer _line;
    private GradientAlphaKey[] _alphaKeys;
    private GradientColorKey[] _colorKeys;
    private Gradient _gradient;
    private float _timeLeft;

    public Sound ShotSound => _shotSound;

    public Sound HitSound => _hitSound;

    public Sound WorldHitSound => _worldHitSound;

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
        _timeLeft -= Time.deltaTime;

        if (_timeLeft > 0f)
            return;

        Released?.Invoke(this);
    }

    public void OnSpawn()
    {
        _timeLeft = _lifetime;
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
        _timeLeft = _lifetime;
    }
}
