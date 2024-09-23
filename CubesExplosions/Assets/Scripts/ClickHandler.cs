using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
//[RequireComponent(typeof(Spawner))]
//[RequireComponent(typeof(Exploder))]

public class ClickHandler : MonoBehaviour
{
    private Spawner _spawner;

    private Exploder _exploder;

    private float _splitChance = 1;

    public void SetSplitChance(float splitChance)
    {
        _splitChance = splitChance;
    }

    private void OnMouseUpAsButton()
    {
        int minNewCubes = 2;
        int maxNewCubes = 6;
        int newCubesAmount = Random.Range(minNewCubes, maxNewCubes + 1);

        _spawner = GetComponent<Spawner>();
        _exploder = GetComponent<Exploder>();

        if (_splitChance >= Random.value)
        {
            _spawner.SpawnCubes(newCubesAmount, _splitChance);
            _exploder.Explode(_spawner.GetExplodibleObjects(), transform.position);
        }

        Destroy(gameObject);
    }
}