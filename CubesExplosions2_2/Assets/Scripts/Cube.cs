using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody), typeof(Color))]
public class Cube : MonoBehaviour
{
    private Color _color;

    public static event Action<Cube> Clicked;

    public Rigidbody Rigidbody { get; private set; }

    [field: SerializeField] public float SplitChance { get; private set; }

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
        _color = GetComponent<Color>();
        SplitChance = 1;
    }

    private void OnMouseUpAsButton()
    {
        Clicked?.Invoke(this);
        Destroy(gameObject);
    }

    public void Init(float splitChance)
    {
        int scaleDecreaser = 2;
        int chanceDecreaser = 2;

        Rigidbody.useGravity = true;
        Rigidbody.interpolation = RigidbodyInterpolation.Extrapolate;
        Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        SplitChance = splitChance / chanceDecreaser;
        transform.localScale /= scaleDecreaser;
        _color.SetColor(UnityEngine.Random.ColorHSV());
    }
}