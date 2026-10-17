using System;
using UnityEngine;

[Serializable]
public class WaveRow
{
    [SerializeField] private string _location;
    [SerializeField] private int _level;
    [SerializeField] private int _order;
    [SerializeField] private int _count;
    [SerializeField] private WaveMode _mode;
    [SerializeField] private string[] _enemies;
    [SerializeField] private float _interval;
    [SerializeField] private float _intervalMax;
    [SerializeField] private int _shieldCapacity;

    public WaveRow(string location, int level, int order, int count, WaveMode mode, string[] enemies,
        float interval, float intervalMax, int shieldCapacity)
    {
        _shieldCapacity = shieldCapacity;
        _location = location;
        _level = level;
        _order = order;
        _count = count;
        _mode = mode;
        _enemies = enemies;
        _interval = interval;
        _intervalMax = intervalMax;
    }

    public string Location => _location;

    public int ShieldCapacity => _shieldCapacity;

    public int Level => _level;

    public int Order => _order;

    public int Count => _count;

    public WaveMode Mode => _mode;

    public string[] Enemies => _enemies;

    public float Interval => _interval;

    public float IntervalMax => _intervalMax;
}
