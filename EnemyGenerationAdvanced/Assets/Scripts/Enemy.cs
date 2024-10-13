using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    private Vector3 _target;

    private float _speed = 5;

    public string Type;

    abstract public void SetParameters();

    public void Init(Vector3 target, string type)
    {
        _target = target;
        Type = type;
        SetParameters();
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, _target, _speed * Time.deltaTime);
    }
}