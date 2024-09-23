using System.Collections.Generic;
using UnityEngine;

public class Exploder : MonoBehaviour
{
    [SerializeField] float _explosionForce = 400;
    [SerializeField] float _explosionRadius = 10;

    public void ExplodeSpawnedObjects(List<Rigidbody> rigidbodies, Vector3 position)
    {
        foreach (Rigidbody rigidbody in rigidbodies)
            rigidbody.AddExplosionForce(_explosionForce, position, _explosionRadius);
    }

    public void ExplodeSelf(Vector3 position)
    {
        SetExplosionParameters();

        foreach (Rigidbody rigidbody in GetExplodableObjects(position))
        {
            float minDistance = 1;
            float distance = Mathf.Max(minDistance, Vector3.Distance(rigidbody.transform.position, transform.position));

            rigidbody.AddExplosionForce(_explosionForce * ApplyDistanceFactor(distance), position, _explosionRadius);
        }
    }

    private void SetExplosionParameters()
    {
        _explosionForce = _explosionForce / transform.localScale.x;
        _explosionRadius = _explosionRadius / transform.localScale.x;
    }

    private List<Rigidbody> GetExplodableObjects(Vector3 position)
    {
        Collider[] hits = Physics.OverlapSphere(position, _explosionRadius);

        List<Rigidbody> rigidbodies = new();

        foreach (Collider hit in hits)
            if (hit.attachedRigidbody != null)
                rigidbodies.Add(hit.attachedRigidbody);

        return rigidbodies;
    }

    private float ApplyDistanceFactor(float distance)
    {
        float maxDistanceFactor = 0.5f;

        return maxDistanceFactor + (1 - maxDistanceFactor) / distance;
    }
}