using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class WarheadBlink : MonoBehaviour
{
    [SerializeField] private float _rate = 4f;
    [SerializeField] private float _dimAlpha = 0.1f;

    private SpriteRenderer _renderer;
    private float _phase;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        _phase = 0f;
    }

    private void Update()
    {
        _phase += _rate * Time.deltaTime;

        Color color = _renderer.color;

        color.a = Mathf.Sin(_phase) > 0f ? 1f : _dimAlpha;
        _renderer.color = color;
    }
}
