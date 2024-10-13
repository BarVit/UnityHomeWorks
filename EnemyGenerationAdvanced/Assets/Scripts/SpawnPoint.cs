using UnityEngine;

public class SpawnPoint<T> : MonoBehaviour where T : Enemy
{
    private T _enemy;

    private Target _target;

    public void SetEnemyType(T t)
    {
        _enemy = t;
    }

    public void SetTarget(Target target)
    {
        _target = target;
    }

    public string GetEnemyType()
    {
        return _enemy.Type;
    }
}