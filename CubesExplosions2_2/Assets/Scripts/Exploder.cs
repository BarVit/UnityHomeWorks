using System.Collections.Generic;
using UnityEngine;

public class Exploder : MonoBehaviour
{
    [SerializeField] private float _explosionForce = 200;
    [SerializeField] private float _explosionRadius = 10;

    public void ExplodeSpawnedObjects(List<Rigidbody> rigidbodies, Vector3 position)
    {
        foreach (Rigidbody rigidbody in rigidbodies)
        {
            rigidbody.AddExplosionForce(_explosionForce / rigidbody.transform.localScale.x, position,
                _explosionRadius / rigidbody.transform.localScale.x);
        }
    }

    public void ExplodeSelf(Transform objectTransform)
    {
        foreach (Rigidbody rigidbody in GetExplodableObjects(objectTransform.position))
        {
            float minDistance = 1;
            float distance = Mathf.Max(minDistance, Vector3.Distance(rigidbody.transform.position, objectTransform.position));
            float actualForce = _explosionForce / objectTransform.localScale.x * ApplyDistanceFactor(distance);
            float actualRadius = _explosionRadius / objectTransform.localScale.x;

            rigidbody.AddExplosionForce(actualForce, objectTransform.position, actualRadius);
        }
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