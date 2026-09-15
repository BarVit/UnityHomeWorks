using System;
using System.Collections;
using UnityEngine;

public class ExplosionAnimation : MonoBehaviour, IPoolable
{
    private const float NotCalculated = -1f;

    [SerializeField] private AudioSource _audioSource;

    private Camera _camera;
    private ParticleSystem[] _particleSystems;
    private Animator _animator;
    private Coroutine _lifetimeCountdown;
    private float _duration = NotCalculated;

    public event Action<IPoolable> Released;

    private float Duration
    {
        get
        {
            if (_duration == NotCalculated)
                _duration = CalculateDuration();

            return _duration;
        }
    }

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

    public void Play()
    {
        if (_animator != null)
        {
            _animator.Rebind();
            _animator.Update(0f);
        }

        foreach (ParticleSystem particleSystem in _particleSystems)
            particleSystem.Play(true);

        if (ScreenArea.IsAudible(_camera, transform.position))
            _audioSource.Play();

        _lifetimeCountdown = StartCoroutine(CountLifetime(Duration));
    }

    private float CalculateDuration()
    {
        float longest = 0f;

        foreach (ParticleSystem particleSystem in GetComponentsInChildren<ParticleSystem>(true))
        {
            ParticleSystem.MainModule main = particleSystem.main;

            longest = Mathf.Max(longest,
                main.startDelay.constantMax + main.duration + main.startLifetime.constantMax);
        }

        Animator animator = GetComponentInChildren<Animator>(true);

        if (animator != null && animator.runtimeAnimatorController != null)
            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
                longest = Mathf.Max(longest, clip.length);

        if (_audioSource != null && _audioSource.clip != null)
            longest = Mathf.Max(longest, _audioSource.clip.length);

        return longest;
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
