using UnityEngine;
using System;

[RequireComponent(typeof(CubeColor), typeof(Rigidbody))]
public class Cube : MonoBehaviour
{
    private CubeColor _color;
    private Rigidbody _rigidbody;

    private bool _isFirstCollision = true;

    public static event Action<Cube> Collided;

    private void Awake()
    {
        _color = GetComponent<CubeColor>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.GetComponent<Plane>() != null && _isFirstCollision)
        {
            _color.SetColor(UnityEngine.Random.ColorHSV());
            _isFirstCollision = false;
            Collided?.Invoke(this);
        }
    }

    public void Init()
    {
        _isFirstCollision = true;
        _color.SetColor(Color.blue);
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        transform.rotation = Quaternion.identity;
    }
}