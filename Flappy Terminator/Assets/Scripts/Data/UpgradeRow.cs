using System;
using UnityEngine;

[Serializable]
public class UpgradeRow
{
    [SerializeField] private UpgradeStat _stat;
    [SerializeField] private int _level;
    [SerializeField] private float _bonus;
    [SerializeField] private int _price;

    public UpgradeRow(UpgradeStat stat, int level, float bonus, int price)
    {
        _stat = stat;
        _level = level;
        _bonus = bonus;
        _price = price;
    }

    public UpgradeStat Stat => _stat;

    public int Level => _level;

    public float Bonus => _bonus;

    public int Price => _price;
}
