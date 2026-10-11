using System;
using UnityEngine;

[Serializable]
public class LevelRow<TStat> where TStat : struct, Enum
{
    [SerializeField] private string _owner;
    [SerializeField] private TStat _stat;
    [SerializeField] private int _level;
    [SerializeField] private float _bonus;
    [SerializeField] private int _price;

    public LevelRow(string owner, TStat stat, int level, float bonus, int price)
    {
        _owner = owner;
        _stat = stat;
        _level = level;
        _bonus = bonus;
        _price = price;
    }

    public string Owner => _owner;

    public TStat Stat => _stat;

    public int Level => _level;

    public float Bonus => _bonus;

    public int Price => _price;
}
