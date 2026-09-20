using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BlastWave : MonoBehaviour, IPoolable
{
    [SerializeField] private float _lifetime = 0.35f;

    private SpriteRenderer _renderer;
    private float _fromScale;
    private float _toScale;
    private float _timeLeft;

    public event Action<IPoolable> Released;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (_timeLeft <= 0f)
            return;

        _timeLeft -= Time.deltaTime;

        if (_timeLeft <= 0f)
        {
            Released?.Invoke(this);

            return;
        }

        Show(1f - _timeLeft / _lifetime);
    }

    public void OnSpawn()
    {
        _timeLeft = 0f;
    }

    public void OnDespawn()
    {
    }

    public void Play(float fromDiameter, float toDiameter)
    {
        float width = _renderer.sprite == null ? 1f : _renderer.sprite.bounds.size.x;

        _fromScale = fromDiameter / width;
        _toScale = toDiameter / width;
        _timeLeft = _lifetime;
        Show(0f);
    }

    private void Show(float grown)
    {
        float scale = Mathf.Lerp(_fromScale, _toScale, grown);
        Color color = _renderer.color;

        color.a = 1f - grown * grown;
        _renderer.color = color;
        transform.localScale = new Vector3(scale, scale, 1f);
    }
}
