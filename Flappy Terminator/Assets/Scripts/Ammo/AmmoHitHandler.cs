using System;
using System.Collections.Generic;
using UnityEngine;

public class AmmoHitHandler : MonoBehaviour
{
    [SerializeField] private List<int> _possibleAmmoLayers;

    public event Action<Ammo> HitDetected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Ammo ammo))
        {
            if (_possibleAmmoLayers.Contains(ammo.gameObject.layer))
            {
                HitDetected?.Invoke(ammo);
                ammo.Hit(other.ClosestPoint(transform.position));
                ammo.Remove();
            }
        }
    }
}