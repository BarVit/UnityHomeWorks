using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody), typeof(Renderer))]
public class ClickHandler : MonoBehaviour
{
    [SerializeField] private Spawner _spawner;

    [SerializeField] private Exploder _exploder;

    private List<Rigidbody> _spawnedRigidbodies = new();

    private Rigidbody _rigidbody;

    private float _splitChance = 1;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void OnMouseUpAsButton()
    {
        int minNewCubes = 2;
        int maxNewCubes = 6;
        int newCubesAmount = Random.Range(minNewCubes, maxNewCubes + 1);

        if (_splitChance >= Random.value)
        {
            _spawnedRigidbodies = _spawner.Spawn(newCubesAmount, gameObject, _splitChance);
            _exploder.ExplodeSpawnedObjects(_spawnedRigidbodies, transform.position);
        }
        else
        {
            _exploder.ExplodeSelf(transform);
        }

        Destroy(gameObject);
    }

    public void SetParameters(float splitChance)
    {
        int scaleDecreaser = 2;
        int chanceDecreaser = 2;

        _rigidbody.useGravity = true;
        _rigidbody.interpolation = RigidbodyInterpolation.Extrapolate;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        _splitChance = splitChance / chanceDecreaser;
        transform.localScale /= scaleDecreaser;
        GetComponent<Renderer>().material.color = Random.ColorHSV();
    }

    public Rigidbody GetRigidbody()
    {
        return _rigidbody;
    }
}