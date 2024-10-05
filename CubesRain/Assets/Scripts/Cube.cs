using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody), typeof(Renderer), typeof(ColorChanger))]
public class Cube : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private Material _material;
    private ColorChanger _colorChanger;

    private bool _isFirstCollision = true;

    public event Action<Cube> Collided;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _material = GetComponent<Renderer>().material;
        _colorChanger = GetComponent<ColorChanger>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.GetComponent<Plane>() != null && _isFirstCollision)
        {
            _colorChanger.SetRandomColor(_material);
            _isFirstCollision = false;
            Collided?.Invoke(this);
        }
    }

    public void Init()
    {
        _isFirstCollision = true;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        transform.rotation = Quaternion.identity;
        _colorChanger.SetOriginalColor(_material);
    }
}