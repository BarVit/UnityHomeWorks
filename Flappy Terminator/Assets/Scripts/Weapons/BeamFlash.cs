using System;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BeamFlash : MonoBehaviour, IPoolable
{
    [SerializeField] private float _lifetime = 0.08f;
    [SerializeField] private Sound _shotSound;

    private LineRenderer _line;
    private float _timeLeft;

    public Sound ShotSound => _shotSound;

    public event Action<IPoolable> Released;

    private void Awake()
    {
        _line = GetComponent<LineRenderer>();
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

    public void Show(Vector3 from, Vector3 to)
    {
        _line.useWorldSpace = true;
        _line.positionCount = 2;
        _line.SetPosition(0, from);
        _line.SetPosition(1, to);
        _timeLeft = _lifetime;
    }
}
