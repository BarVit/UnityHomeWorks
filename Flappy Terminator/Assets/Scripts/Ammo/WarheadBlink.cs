using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class WarheadBlink : MonoBehaviour
{
    [SerializeField] private float _onDuration = 0.25f;
    [SerializeField] private float _offDuration = 0.25f;
    [SerializeField] private float _fadeDuration = 0.05f;
    [SerializeField] private float _dimAlpha = 0.1f;

    private SpriteRenderer _renderer;
    private float _time;

    private float Cycle => _onDuration + _offDuration + _fadeDuration * 2f;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        _time = 0f;
    }

    private void Update()
    {
        _time = Cycle <= 0f ? 0f : (_time + Time.deltaTime) % Cycle;

        Color color = _renderer.color;

        color.a = Mathf.Lerp(_dimAlpha, 1f, GetLight(_time));
        _renderer.color = color;
    }

    private float GetLight(float time)
    {
        if (time < _onDuration)
            return 1f;

        time -= _onDuration;

        if (time < _fadeDuration)
            return 1f - time / _fadeDuration;

        time -= _fadeDuration;

        if (time < _offDuration)
            return 0f;

        time -= _offDuration;

        return _fadeDuration <= 0f ? 1f : Mathf.Clamp01(time / _fadeDuration);
    }
}
