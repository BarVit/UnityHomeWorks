using System.Collections;
using UnityEngine;

public class SmoothHealthBar : MonoBehaviour
{
    [SerializeField] private Health _health;
    [SerializeField] private RectTransform _healthFill;
    [SerializeField] private RectTransform _damageFill;
    [SerializeField] private float _healthFallTime = 0.15f;
    [SerializeField] private float _damageHoldTime = 1f;
    [SerializeField] private float _damageFallTime = 0.35f;

    private Coroutine _healthFalling;
    private Coroutine _damageFalling;

    private void OnEnable()
    {
        _health.Changed += OnHealthChanged;
    }

    private void Start()
    {
        SetInstantly(GetPart(_health.Value));
    }

    private void OnDisable()
    {
        _health.Changed -= OnHealthChanged;
    }

    private void OnHealthChanged(int value)
    {
        float target = GetPart(value);

        StopFalling(ref _healthFalling);
        _healthFalling = StartCoroutine(Fall(_healthFill, target, _healthFallTime, 0f));

        StopFalling(ref _damageFalling);

        if (target >= GetPart(_damageFill))
        {
            SetPart(_damageFill, target);

            return;
        }

        _damageFalling = StartCoroutine(Fall(_damageFill, target, _damageFallTime, _damageHoldTime));
    }

    private float GetPart(int value)
    {
        return _health.MaxValue > 0 ? (float)value / _health.MaxValue : 0f;
    }

    private float GetPart(RectTransform fill)
    {
        return fill.anchorMax.x;
    }

    private void SetPart(RectTransform fill, float part)
    {
        Vector2 anchorMax = fill.anchorMax;

        anchorMax.x = part;
        fill.anchorMax = anchorMax;
    }

    private void SetInstantly(float part)
    {
        StopFalling(ref _healthFalling);
        StopFalling(ref _damageFalling);

        SetPart(_healthFill, part);
        SetPart(_damageFill, part);
    }

    private void StopFalling(ref Coroutine falling)
    {
        if (falling == null)
            return;

        StopCoroutine(falling);
        falling = null;
    }

    private IEnumerator Fall(RectTransform fill, float target, float time, float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        float start = GetPart(fill);

        for (float elapsed = 0f; elapsed < time; elapsed += Time.deltaTime)
        {
            SetPart(fill, Mathf.Lerp(start, target, elapsed / time));

            yield return null;
        }

        SetPart(fill, target);
    }
}
