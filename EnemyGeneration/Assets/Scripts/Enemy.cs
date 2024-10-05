using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Vector3 _direction = Vector3.zero;

    private float _speed = 5;

    public void Init(Vector3 direction)
    {
        _direction = direction;
    }

    private void Update()
    {
        transform.position += _direction.normalized * _speed * Time.deltaTime;
    }
}