using System.Collections.Generic;
using UnityEngine;

public class OverheatSmoke : MonoBehaviour
{
    [SerializeField] private PlayerShooter _shooter;
    [SerializeField] private ParticleSystem _smokePrefab;

    private readonly List<ParticleSystem> _smokes = new List<ParticleSystem>();

    private bool _isSmoking;

    private void Start()
    {
        foreach (Transform mount in _shooter.Mounts)
            _smokes.Add(Instantiate(_smokePrefab, mount.position, mount.rotation, mount));
    }

    private void Update()
    {
        if (_shooter.IsOverheated == _isSmoking)
            return;

        _isSmoking = _shooter.IsOverheated;

        foreach (ParticleSystem smoke in _smokes)
            Toggle(smoke);
    }

    private void Toggle(ParticleSystem smoke)
    {
        if (_isSmoking)
            smoke.Play(true);
        else
            smoke.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}
