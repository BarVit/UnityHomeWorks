using System.Collections.Generic;
using UnityEngine;

public class Exploder : MonoBehaviour
{
    [SerializeField] private float _explosionForce = 1500;
    [SerializeField] private float _explosionRadius = 10;

    public void Explode(List<Rigidbody> rigidbodies, Vector3 position)
    {
        foreach (Rigidbody rigidbody in rigidbodies)
            rigidbody.AddExplosionForce(_explosionForce, position, _explosionRadius);
    }
}