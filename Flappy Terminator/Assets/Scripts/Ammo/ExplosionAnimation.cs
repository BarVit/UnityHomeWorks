using System;
using System.Collections;
using UnityEngine;

public class ExplosionAnimation : MonoBehaviour, IPoolable
{
    [SerializeField] private AudioSource _audioSource;

    private Camera _camera;
    private ParticleSystem[] _particleSystems;
    private Animator _animator;
    private Coroutine _lifetimeCountdown;

    public event Action<IPoolable> Released;

    private void Awake()
    {
        _camera = Camera.main;
        _particleSystems = GetComponentsInChildren<ParticleSystem>(true);
        _animator = GetComponentInChildren<Animator>(true);
    }

    public void OnSpawn()
    {
        StopParticles();
    }

    public void OnDespawn()
    {
        if (_lifetimeCountdown != null)
        {
            StopCoroutine(_lifetimeCountdown);
            _lifetimeCountdown = null;
        }

        StopParticles();
    }

    public void Play(float lifetime)
    {
        if (_animator != null)
        {
            _animator.Rebind();
            _animator.Update(0f);
        }

        foreach (ParticleSystem particleSystem in _particleSystems)
            particleSystem.Play(true);

        if (ScreenArea.Contains(_camera, transform.position))
            _audioSource.Play();

        _lifetimeCountdown = StartCoroutine(CountLifetime(lifetime));
    }

    private void StopParticles()
    {
        foreach (ParticleSystem particleSystem in _particleSystems)
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private IEnumerator CountLifetime(float lifetime)
    {
        yield return new WaitForSeconds(lifetime);

        _lifetimeCountdown = null;
        Released?.Invoke(this);
    }
}
