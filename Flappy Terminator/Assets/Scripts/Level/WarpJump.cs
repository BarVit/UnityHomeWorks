using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpJump : MonoBehaviour
{
    [SerializeField] private PlayerShip _playerShip;
    [SerializeField] private SpriteRenderer _shipSprite;
    [SerializeField] private ParticleSystem _warpEffect;
    [SerializeField] private Vector2 _warpOffset = new(3.5f, 0f);
    [SerializeField] private float _alignDuration = 0.6f;
    [SerializeField, Range(0f, 1f)] private float _stretchAtWarpProgress = 0.75f;
    [SerializeField] private float _stretchDuration = 0.4f;
    [SerializeField] private float _stretchFactor = 2f;
    [SerializeField] private float _dashDuration = 0.15f;
    [SerializeField] private float _dashDistance = 16f;
    [SerializeField] private float _tailDuration = 1f;

    private Coroutine _jumping;

    public event Action Finished;

    public void Play()
    {
        if (_jumping != null)
            return;

        _jumping = StartCoroutine(Jump());
    }

    private IEnumerator Jump()
    {
        Transform ship = _playerShip.transform;

        _playerShip.DisableControl();

        yield return Align(ship);

        _warpEffect.transform.position = ship.position + (Vector3)_warpOffset;
        _warpEffect.Play();

        yield return new WaitForSeconds(_warpEffect.main.duration * _stretchAtWarpProgress);

        yield return Stretch(ship);

        yield return Dash(ship);

        _shipSprite.enabled = false;

        yield return new WaitForSeconds(_tailDuration);

        _jumping = null;
        Finished?.Invoke();
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
