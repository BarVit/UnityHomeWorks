using UnityEngine;
using System.Collections.Generic;

public class Cube : MonoBehaviour
{
    [SerializeField] private GameObject _prefab;

    [SerializeField] private float _splitChance = 1;
    [SerializeField] private float _explosionForce = 1500;
    [SerializeField] private float _explosionRadius = 10;

    private List<GameObject> _createdCubes = new();

    private void OnMouseUpAsButton()
    {
        int minNewCubes = 2;
        int maxNewCubes = 6;
        int newCubesAmount = Random.Range(minNewCubes, maxNewCubes + 1);

        if (_splitChance >= Random.value)
        {
            for (int i = 0; i < newCubesAmount; i++)
                SpawnCube(RandomizeSpawnPosition());

            Explode();
        }

        Destroy(gameObject);
    }

    private void Explode()
    {
        foreach (GameObject cube in _createdCubes)
            cube.GetComponent<Rigidbody>().AddExplosionForce(_explosionForce, transform.position, _explosionRadius);
    }

    private void SpawnCube(Vector3 position)
    {
        GameObject cube = Instantiate(_prefab, position, Quaternion.identity);

        Rigidbody rigidbody = cube.GetComponent<Rigidbody>();

        int scaleDecreaser = 2;
        int chanceDecreaser = 2;

        cube.GetComponent<Cube>().SetSplitChance(_splitChance / chanceDecreaser);
        cube.GetComponent<Cube>()._prefab = _prefab;
        cube.GetComponent<Renderer>().material.color = Random.ColorHSV();
        cube.transform.position = position;
        cube.transform.localScale = transform.localScale / scaleDecreaser;
        rigidbody.useGravity = true;
        rigidbody.interpolation = RigidbodyInterpolation.Extrapolate;
        rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        _createdCubes.Add(cube);
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

    public void SetSplitChance(float splitChance)
    {
        _splitChance = splitChance;
    }
}