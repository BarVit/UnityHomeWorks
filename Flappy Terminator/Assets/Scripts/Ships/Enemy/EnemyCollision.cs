using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCollision : MonoBehaviour
{
    [SerializeField] private EnemyShip _enemyShip;

    public void Bump()
    {
        _enemyShip.Die();
    }

    public EnemyShip GetShip()
    {
        return _enemyShip;
    }
}
