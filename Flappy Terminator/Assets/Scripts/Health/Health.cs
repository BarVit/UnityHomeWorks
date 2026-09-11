using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [field: SerializeField] public int Value { get; private set; }
    [field: SerializeField] public int MaxValue { get; private set; }

    public event Action<int> Changed;
    public event Action Died;

    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        Value = MaxValue;
    }

    public void TakeDamage(int damage)
    {
        if (damage >= 0)
        {
            Value = Mathf.Clamp(Value - damage, 0, Value);
            Changed?.Invoke(Value);
        }

        if (Value <= 0)
            Die();
    }

    public void Die()
    {
        Died?.Invoke();
    }
}