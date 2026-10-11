using System;
using UnityEngine;

public class AmmoHitHandler : MonoBehaviour
{
    [SerializeField] private LayerMask _ammoLayers;

    private IDamageable _owner;

    public event Action<Ammo> HitDetected;

    private void Awake()
    {
        _owner = GetComponentInParent<IDamageable>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Ammo ammo) == false)
            return;

        if (ammo.IsArmed == false)
            return;

        if (ammo.IsPersistent)
            return;

        if (_owner != null && _owner.IsVulnerable == false)
            return;

        if (Contains(ammo.gameObject.layer) == false)
            return;

        ammo.Disarm();
        HitDetected?.Invoke(ammo);
        ammo.Hit(other.ClosestPoint(transform.position), gameObject);
        ammo.Remove();
    }

    private bool Contains(int layer)
    {
        return (_ammoLayers.value & (1 << layer)) != 0;
    }
}
