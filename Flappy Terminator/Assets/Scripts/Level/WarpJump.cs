using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpJump : MonoBehaviour
{
    [SerializeField] private PlayerShip _playerShip;
    [SerializeField] private ParticleSystem _warpEffect;
    [SerializeField] private GameObject _skipHint;
    [SerializeField] private ExpandingFade _shockWave;
    [SerializeField] private ExpandingFade _flash;
    [SerializeField] private Vector2 _engineOffset = new(-0.6f, 0f);
    [SerializeField] private Vector2 _warpOffset = new(3.5f, 0f);
    [SerializeField] private float _alignDuration = 0.6f;
    [SerializeField, Range(0f, 1f)] private float _stretchAtWarpProgress = 0.75f;
    [SerializeField] private float _stretchDuration = 0.4f;
    [SerializeField] private float _stretchFactor = 2f;
    [SerializeField] private float _dashDuration = 0.15f;
    [SerializeField] private float _dashDistance = 16f;
    [SerializeField] private AudioSource _sound;
    [SerializeField] private float _soundFadeDuration = 0.5f;

    private Coroutine _jumping;
    private Coroutine _fading;
    private float _soundVolume;

    private float EffectDuration
    {
        get
        {
            ParticleSystem.MainModule main = _warpEffect.main;

            return main.duration + main.startLifetime.constantMax;
        }
    }

    private float ActionsDuration =>
        _warpEffect.main.duration * _stretchAtWarpProgress + _stretchDuration + _dashDuration;

    private float TailDuration => Mathf.Max(0f, EffectDuration - ActionsDuration);

    public bool IsPlaying => _jumping != null;

    public event Action Finished;

    private void Awake()
    {
        if (_sound != null)
            _soundVolume = _sound.volume;
    }

    public void Play()
    {
        if (_jumping != null)
            return;

        StartSound();
        _jumping = StartCoroutine(Jump());
    }

    public void Skip()
    {
        if (_jumping == null)
            return;

        Stop();
        Finished?.Invoke();
    }

    public void Stop()
    {
        if (_jumping != null)
        {
            StopCoroutine(_jumping);
            _jumping = null;
        }

        StopWarpEffect();
        _flash.Stop();
        _shockWave.Stop();
        _skipHint.SetActive(false);
        FadeSound();
    }

    private IEnumerator Jump()
    {
        Transform ship = _playerShip.transform;

        _playerShip.DisableControl();
        _skipHint.SetActive(true);

        yield return Align(ship);

        _warpEffect.transform.position = ship.position + (Vector3)_warpOffset;
        StopWarpEffect();
        _warpEffect.Play();

        yield return new WaitForSeconds(_warpEffect.main.duration * _stretchAtWarpProgress);

        yield return Stretch(ship);

        _flash.PlayAttached(ship, _engineOffset);

        yield return Dash(ship);

        _playerShip.Hide();
        _shockWave.Play(ship.position);

        yield return new WaitForSeconds(TailDuration);

        StopWarpEffect();
        _skipHint.SetActive(false);
        FadeSound();

        _jumping = null;
        Finished?.Invoke();
    }

    private void StartSound()
    {
        if (_sound == null)
            return;

        if (_fading != null)
        {
            StopCoroutine(_fading);
            _fading = null;
        }

        _sound.volume = _soundVolume;
        _sound.Play();
    }

    private void FadeSound()
    {
        if (_sound == null || _sound.isPlaying == false || _fading != null)
            return;

        _fading = StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float startVolume = _sound.volume;

        foreach (float progress in Progress(_soundFadeDuration))
        {
            _sound.volume = Mathf.Lerp(startVolume, 0f, progress);

            yield return null;
        }

        _sound.Stop();
        _sound.volume = _soundVolume;
        _fading = null;
    }

    private void StopWarpEffect()
    {
        _warpEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private IEnumerator Align(Transform ship)
    {
        Quaternion startRotation = ship.rotation;

        foreach (float progress in Progress(_alignDuration))
        {
            ship.rotation = Quaternion.Lerp(startRotation, Quaternion.identity, progress);

            yield return null;
        }
    }

    private IEnumerator Stretch(Transform ship)
    {
        Vector3 startScale = ship.localScale;
        Vector3 targetScale = new(startScale.x * _stretchFactor, startScale.y, startScale.z);

        foreach (float progress in Progress(_stretchDuration))
        {
            ship.localScale = Vector3.Lerp(startScale, targetScale, progress);

            yield return null;
        }
    }

    private IEnumerator Dash(Transform ship)
    {
        Vector3 startPosition = ship.position;
        Vector3 targetPosition = startPosition + ship.right * _dashDistance;

        foreach (float progress in Progress(_dashDuration))
        {
            ship.position = Vector3.Lerp(startPosition, targetPosition, progress * progress);

            yield return null;
        }
    }

    private IEnumerable<float> Progress(float duration)
    {
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
            yield return elapsed / duration;

        yield return 1f;
    }
}
