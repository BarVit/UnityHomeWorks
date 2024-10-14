using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    private float _speed = 5;
    private Vector3 _targetPosition;

    public abstract void Init();

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _speed * Time.deltaTime);
    }

    public void SetTarget(Vector3 target)
    {
        _targetPosition = target;
    }
}