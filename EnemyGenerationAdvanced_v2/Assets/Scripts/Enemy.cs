using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    private float _speed = 5;
    private Transform _target;

    public abstract void Init();

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, _target.position, _speed * Time.deltaTime);
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }
}