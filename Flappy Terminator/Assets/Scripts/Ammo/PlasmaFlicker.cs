using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlasmaFlicker : MonoBehaviour
{
    [SerializeField] private float _speed = 7f;
    [SerializeField] private float _depth = 0.25f;

    private SpriteRenderer _renderer;
    private float _baseAlpha;
    private float _phase;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _baseAlpha = _renderer.color.a;
    }

    private void OnEnable()
    {
        _phase = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        _phase += _speed * Time.deltaTime;

        Color color = _renderer.color;

        color.a = Mathf.Clamp01(_baseAlpha * (1f - _depth + _depth * Mathf.Abs(Mathf.Sin(_phase))));
        _renderer.color = color;
    }
}
