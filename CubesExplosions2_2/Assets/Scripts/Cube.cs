using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody), typeof(Renderer))]
public class Cube : MonoBehaviour
{
    private Rigidbody _rigidbody;

    [field: SerializeField] public float SplitChance { get; private set; }

    public event Action<Cube> Clicked;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        SplitChance = 1;
    }

    private void OnMouseUpAsButton()
    {
        Clicked?.Invoke(this);
        Destroy(gameObject);
    }

    public void SetParameters(float splitChance)
    {
        int scaleDecreaser = 2;
        int chanceDecreaser = 2;

        _rigidbody.useGravity = true;
        _rigidbody.interpolation = RigidbodyInterpolation.Extrapolate;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        SplitChance = splitChance / chanceDecreaser;
        transform.localScale /= scaleDecreaser;
        GetComponent<Renderer>().material.color = UnityEngine.Random.ColorHSV();
    }

    public Rigidbody GetRigidbody()
    {
        return _rigidbody;
    }
}