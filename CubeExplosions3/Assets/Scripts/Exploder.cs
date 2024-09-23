using System.Collections.Generic;
using UnityEngine;

public class Exploder : MonoBehaviour
{
    public float _explosionForce = 200;
    public float _explosionRadius = 10;

    public void ExplodeSpawnedObjects(List<Rigidbody> rigidbodies, Vector3 position)
    {
        foreach (Rigidbody rigidbody in rigidbodies)
            rigidbody.AddExplosionForce(_explosionForce, position, _explosionRadius);
    }

    public void ExplodeSelf(Vector3 position)
    {
        SetExplosionParams();

        foreach (Rigidbody rigidbody in GetExplodableObjects(position))
        {
            float minDistance = 1;
            float distance = Vector3.Distance(rigidbody.transform.position, transform.position);

            if (distance < 1)
                distance = minDistance;

            _explosionForce = _explosionForce * ApplyDistanceFactor(distance);
            rigidbody.AddExplosionForce(_explosionForce, position, _explosionRadius);
        }
    }

    public void SetExplosionParams()
    {
        _explosionForce = _explosionForce / transform.localScale.x;
        _explosionRadius = _explosionRadius / transform.localScale.x;
    }

    private List<Rigidbody> GetExplodableObjects(Vector3 position)
    {
        Collider[] hits = Physics.OverlapSphere(position, _explosionRadius);

        List<Rigidbody> rigidbodies = new();

        foreach (Collider hit in hits)
            if(hit.attachedRigidbody != null)
                rigidbodies.Add(hit.attachedRigidbody);

        return rigidbodies;
    }

    private float ApplyDistanceFactor(float distance)
    {
        float maxDistanceDivider = 0.5f;

        return maxDistanceDivider + (1 - maxDistanceDivider) / distance;
    }
}