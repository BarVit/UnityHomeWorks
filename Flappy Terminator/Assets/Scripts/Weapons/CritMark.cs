using System;
using TMPro;
using UnityEngine;

public class CritMark : MonoBehaviour, IPoolable
{
    private const float PopShare = 0.15f;
    private const float FadeShare = 0.55f;

    [SerializeField] private SpriteRenderer _burst;
    [SerializeField] private TMP_Text _label;
    [SerializeField] private float _lifetime = 0.7f;
    [SerializeField] private float _rise = 0.6f;
    [SerializeField] private float _popScale = 1.4f;
    [SerializeField] private float _burstSpin = 90f;

    private Vector3 _origin;
    private float _age;

    public event Action<IPoolable> Released;

    private void Update()
    {
        _age += Time.deltaTime;

        float share = Mathf.Clamp01(_age / _lifetime);

        Animate(share);

        if (share >= 1f)
            Released?.Invoke(this);
    }

    public void OnSpawn()
    {
        _age = 0f;
    }

    public void OnDespawn()
    {
    }

    public void Show(int damage)
    {
        _label.text = damage.ToString();
        Begin();
    }

    public void Show()
    {
        _label.text = string.Empty;
        Begin();
    }

    private void Begin()
    {
        _origin = transform.position;
        _age = 0f;
        Animate(0f);
    }

    private void Animate(float share)
    {
        float pop = share < PopShare
            ? Mathf.Lerp(0.5f, _popScale, share / PopShare)
            : Mathf.Lerp(_popScale, 1f, (share - PopShare) / (1f - PopShare));
        float alpha = share < FadeShare ? 1f : 1f - (share - FadeShare) / (1f - FadeShare);

        transform.position = _origin + Vector3.up * (_rise * share);
        transform.localScale = Vector3.one * pop;

        _burst.transform.localRotation = Quaternion.Euler(0f, 0f, _burstSpin * share);
        _burst.transform.localScale = Vector3.one * (1f - share * 0.6f);
        _burst.color = new Color(1f, 1f, 1f, alpha * (1f - share));
        _label.alpha = alpha;
    }
}
