using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public List<Rigidbody> Spawn(int amount, GameObject cube, float splitChance)
    {
        List<Rigidbody> rigidbodies = new();

        for (int i = 0; i < amount; i++)
        {
            GameObject newCube = Instantiate(cube, RandomizePosition(cube.transform.position), Quaternion.identity);

            ClickHandler clickHandler = newCube.GetComponent<ClickHandler>();

            clickHandler.SetParameters(splitChance);
            rigidbodies.Add(clickHandler.GetRigidbody());
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