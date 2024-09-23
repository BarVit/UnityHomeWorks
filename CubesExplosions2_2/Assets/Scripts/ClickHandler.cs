using UnityEngine;

[RequireComponent(typeof(Spawner))]
[RequireComponent(typeof(Exploder))]

public class ClickHandler : MonoBehaviour
{
    private float _splitChance = 1;

    public void SetSplitChance(float splitChance)
    {
        _splitChance = splitChance;
    }

    private void OnMouseUpAsButton()
    {
        Spawner spawner = GetComponent<Spawner>();

        Exploder exploder = GetComponent<Exploder>();

        int minNewCubes = 2;
        int maxNewCubes = 6;
        int newCubesAmount = Random.Range(minNewCubes, maxNewCubes + 1);

        if (_splitChance >= Random.value)
        {
            spawner.SpawnCubes(newCubesAmount, _splitChance);
            exploder.ExplodeSpawnedObjects(spawner.GetExplodibleObjects(), transform.position);
        }
        else
        {
            exploder.ExplodeSelf(transform.position);
        }

        Destroy(gameObject);
    }
}