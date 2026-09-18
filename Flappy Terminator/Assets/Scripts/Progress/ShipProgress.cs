using System;
using UnityEngine;

[Serializable]
public class ShipProgress
{
    [SerializeField] private int _shipLevel = 1;
    [SerializeField] private int _shipXp;

    public int ShipLevel => _shipLevel;

    public int ShipXp => _shipXp;

    public void AddXp(int amount)
    {
        _shipXp += amount;
    }

    public void SpendXp(int amount)
    {
        _shipXp -= amount;
        _shipLevel++;
    }
}
