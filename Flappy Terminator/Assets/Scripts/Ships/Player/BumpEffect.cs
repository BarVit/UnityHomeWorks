using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class BumpEffect : MonoBehaviour, IPoolable
{
    [SerializeField] private float _lifetime = 1.5f;

    private ParticleSystem _particleSystem;
    private Coroutine _lifetimeCountdown;

    public event Action<IPoolable> Released;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    public void OnSpawn()
    {
        StopParticles();
    }

    public void OnDespawn()
    {
        StopCountdown();
        StopParticles();
    }

    public void Play(float gravityModifier)
    {
        ParticleSystem.MainModule main = _particleSystem.main;

        main.gravityModifier = gravityModifier;

        _particleSystem.Play(true);
        _lifetimeCountdown = StartCoroutine(CountLifetime());
    }

    private void StopParticles()
    {
        _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void StopCountdown()
    {
        if (_lifetimeCountdown == null)
            return;

        StopCoroutine(_lifetimeCountdown);
        _lifetimeCountdown = null;
    }

    private IEnumerator CountLifetime()
    {
        yield return new WaitForSeconds(_lifetime);

        _lifetimeCountdown = null;
        Released?.Invoke(this);
    }
}
