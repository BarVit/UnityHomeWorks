using System.Collections;
using UnityEngine;

public class SmoothHealthBar : MonoBehaviour
{
    [SerializeField] private Health _health;
    [SerializeField] private RectTransform _healthFill;
    [SerializeField] private RectTransform _damageFill;
    [SerializeField] private float _damageHoldTime = 1f;
    [SerializeField] private float _damageFallTime = 0.35f;

    private Coroutine _damageFalling;
    private float _damageTarget;

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

        SetPart(_healthFill, target);
        _damageTarget = target;

        if (target >= GetPart(_damageFill))
        {
            StopFalling();
            SetPart(_damageFill, target);

            return;
        }

        if (_damageFalling == null)
            _damageFalling = StartCoroutine(Chase());
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
        StopFalling();
        _damageTarget = part;
        SetPart(_healthFill, part);
        SetPart(_damageFill, part);
    }

    private void StopFalling()
    {
        if (_damageFalling == null)
            return;

        StopCoroutine(_damageFalling);
        _damageFalling = null;
    }

    private IEnumerator Chase()
    {
        if (_damageHoldTime > 0f)
            yield return new WaitForSeconds(_damageHoldTime);

        float fallTime = Mathf.Max(_damageFallTime, Mathf.Epsilon);
        float speed = 0f;
        float idle = 0f;

        while (idle <= _damageHoldTime)
        {
            float gap = GetPart(_damageFill) - _damageTarget;

            if (gap <= 0f)
            {
                speed = 0f;
                idle += Time.deltaTime;
            }
            else
            {
                idle = 0f;
                speed = Mathf.Max(speed, gap / fallTime);
                SetPart(_damageFill, Mathf.MoveTowards(GetPart(_damageFill), _damageTarget, speed * Time.deltaTime));
            }

            yield return null;
        }

        _damageFalling = null;
    }
}
