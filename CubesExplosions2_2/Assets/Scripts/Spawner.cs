using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private Exploder _exploder;

    private void OnEnable()
    {
        Cube.Clicked += Spawn;
    }

    private void OnDisable()
    {
        Cube.Clicked -= Spawn;
    }

    public void Spawn(Cube cube)
    {
        int minAmount = 2;
        int maxAmount = 6;
        int newCubesAmount = Random.Range(minAmount, maxAmount + 1);

        if (cube.SplitChance >= Random.value)
            _exploder.ExplodeSpawnedObjects(Generate(newCubesAmount, cube), cube.transform.position);
        else
            _exploder.ExplodeSelf(cube.transform);
    }

    public List<Rigidbody> Generate(int amount, Cube cube)
    {
        List<Rigidbody> rigidbodies = new();

        for (int i = 0; i < amount; i++)
        {
            Cube newCube = Instantiate(cube, RandomizePosition(cube.transform.position), Quaternion.identity);

            newCube.Init(cube.SplitChance);
            rigidbodies.Add(newCube.Rigidbody);
        }

        return rigidbodies;
    }

    private Vector3 RandomizePosition(Vector3 position)
    {
        return new Vector3(Disperce(position.x), Disperce(position.y),
            Disperce(position.z));
    }

    private float Disperce(float number)
    {
        float dispersion = 0.05f;

        return number + Random.Range(-dispersion, dispersion);
    }
}