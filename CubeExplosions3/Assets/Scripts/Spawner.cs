using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(ClickHandler))]
//[RequireComponent(typeof(Renderer))]

public class Spawner : MonoBehaviour
{
    private List<Rigidbody> _rigidBodiesCreatedCubes = new();

    public void SpawnCubes(int amount, float splitChance)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject cube = Instantiate(gameObject, RandomizeSpawnPosition(), Quaternion.identity);

            Rigidbody rigidbody = cube.GetComponent<Rigidbody>();

            int scaleDecreaser = 2;
            int chanceDecreaser = 2;

            cube.GetComponent<ClickHandler>().SetSplitChance(splitChance / chanceDecreaser);
            cube.GetComponent<Renderer>().material.color = Random.ColorHSV();
            cube.transform.localScale = transform.localScale / scaleDecreaser;
            SetupRigidBody(rigidbody);
            _rigidBodiesCreatedCubes.Add(rigidbody);
        }
    }

    public List<Rigidbody> GetExplodibleObjects()
    {
        List<Rigidbody> rigidBodiesSpawnedCubes = new();

        foreach (Rigidbody rigidbody in _rigidBodiesCreatedCubes)
            rigidBodiesSpawnedCubes.Add(rigidbody);

        return rigidBodiesSpawnedCubes;
    }

    private void SetupRigidBody(Rigidbody rigidbody)
    {
        rigidbody.useGravity = true;
        rigidbody.interpolation = RigidbodyInterpolation.Extrapolate;
        rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

    private Vector3 RandomizeSpawnPosition()
    {
        return new Vector3(Disperce(transform.position.x), Disperce(transform.position.y),
            Disperce(transform.position.z));
    }

    private float Disperce(float number)
    {
        float dispersion = 0.05f;

        return number * Random.Range(1 - dispersion, 1 + dispersion);
    }
}