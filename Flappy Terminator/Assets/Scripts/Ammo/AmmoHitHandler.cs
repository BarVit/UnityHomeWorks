using System;
using UnityEngine;

public class AmmoHitHandler : MonoBehaviour
{
    [SerializeField] private LayerMask _ammoLayers;

    public event Action<Ammo> HitDetected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Ammo ammo) == false)
            return;

        if (Contains(ammo.gameObject.layer) == false)
            return;

        HitDetected?.Invoke(ammo);
        ammo.Hit(other.ClosestPoint(transform.position));
        ammo.Remove();
    }

    private bool Contains(int layer)
    {
        return (_ammoLayers.value & (1 << layer)) != 0;
    }
}
