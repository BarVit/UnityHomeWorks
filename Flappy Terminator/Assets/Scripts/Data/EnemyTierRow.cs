using System;
using UnityEngine;

[Serializable]
public class EnemyTierRow
{
    [SerializeField] private string _id;
    [SerializeField] private string _ship;
    [SerializeField] private int _tier;
    [SerializeField] private float _healthScale;
    [SerializeField] private float _projectileSpeedScale;
    [SerializeField] private int _barrels;
    [SerializeField] private MovementProfile _movement;

    public EnemyTierRow(string id, string ship, int tier, float healthScale,
        float projectileSpeedScale, int barrels, MovementProfile movement)
    {
        _id = id;
        _ship = ship;
        _tier = tier;
        _healthScale = healthScale;
        _projectileSpeedScale = projectileSpeedScale;
        _barrels = barrels;
        _movement = movement;
    }

    public string Id => _id;

    public string Ship => _ship;

    public int Tier => _tier;

    public float HealthScale => _healthScale;

    public float ProjectileSpeedScale => _projectileSpeedScale;

    public int Barrels => _barrels;

    public MovementProfile Movement => _movement;
}
