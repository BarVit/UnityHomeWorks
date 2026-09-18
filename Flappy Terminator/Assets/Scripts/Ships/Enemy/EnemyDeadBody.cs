using System;
using System.Collections;
using UnityEngine;

public class EnemyDeadBody : MonoBehaviour, IPoolable
{
    [SerializeField] private float _lifetime = 0.4f;

    private Animator _animator;
    private Coroutine _lifetimeCountdown;

    public event Action<IPoolable> Released;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>(true);
    }

    public void OnSpawn()
    {
    }

    public void OnDespawn()
    {
        StopCountdown();
    }

    public void End(RuntimeAnimatorController controller)
    {
        if (_animator != null)
        {
            if (controller != null)
                _animator.runtimeAnimatorController = controller;

            _animator.Rebind();
            _animator.Update(0f);
        }

        _lifetimeCountdown = StartCoroutine(CountLifetime());
    }

    private void StopCountdown()
    {
        if (_lifetimeCountdown == null)
            return;

        StopCoroutine(_lifetimeCountdown);
        _lifetimeCountdown = null;
    }

    private IEnumerator CountLifetime()
    {
        yield return new WaitForSeconds(_lifetime);

        _lifetimeCountdown = null;
        Released?.Invoke(this);
    }
}
