using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class HeatFade : MonoBehaviour
{
    [SerializeField] private Color _hotColor = Color.white;
    [SerializeField] private Color _coolColor = Color.white;
    [SerializeField] private float _hotScale = 1f;
    [SerializeField] private float _coolScale = 1f;
    [SerializeField] private float _duration = 0.3f;

    private SpriteRenderer _renderer;
    private float _timeLeft;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        _timeLeft = _duration;
        Show(0f);
    }

    private void Update()
    {
        if (_timeLeft <= 0f)
            return;

        _timeLeft -= Time.deltaTime;

        Show(_duration <= 0f ? 1f : 1f - Mathf.Clamp01(_timeLeft / _duration));
    }

    private void Show(float cooled)
    {
        float scale = Mathf.Lerp(_hotScale, _coolScale, cooled);

        _renderer.color = Color.Lerp(_hotColor, _coolColor, cooled);
        transform.localScale = new Vector3(scale, scale, 1f);
    }
}
