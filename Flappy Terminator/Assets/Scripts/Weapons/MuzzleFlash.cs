using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MuzzleFlash : MonoBehaviour, IPoolable
{
    [SerializeField] private float _lifetime = 0.06f;
    [SerializeField] private float _startScale = 1f;
    [SerializeField] private float _endScale = 0.35f;
    [SerializeField] private Color _color = Color.white;

    private SpriteRenderer _renderer;
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

        Show(_timeLeft / _lifetime);
    }

    public void OnSpawn()
    {
        _timeLeft = _lifetime;
        Show(1f);
    }

    public void OnDespawn()
    {
    }

    private void Show(float left)
    {
        float scale = Mathf.Lerp(_endScale, _startScale, left);

        transform.localScale = new Vector3(scale, scale, 1f);
        _renderer.color = new Color(_color.r, _color.g, _color.b, _color.a * left);
    }
}
