using System;
using UnityEngine;

[Serializable]
public class EnemyTierRow
{
    [SerializeField] private string _id;
    [SerializeField] private string _ship;
    [SerializeField] private int _tier;
    [SerializeField] private int _health;
    [SerializeField] private string _weapon;
    [SerializeField] private int _barrels;
    [SerializeField] private int _damage;
    [SerializeField] private MovementProfile _movement;
    [SerializeField] private SpriteSkin _skin;

    public EnemyTierRow(string id, string ship, int tier, int health, string weapon, int barrels, int damage,
        MovementProfile movement, SpriteSkin skin)
    {
        _id = id;
        _ship = ship;
        _tier = tier;
        _health = health;
        _weapon = weapon;
        _barrels = barrels;
        _damage = damage;
        _movement = movement;
        _skin = skin;
    }

    public string Id => _id;

    public string Ship => _ship;

    public int Tier => _tier;

    public int Health => _health;

    public string Weapon => _weapon;

    public int Barrels => _barrels;

    public int Damage => _damage;

    public MovementProfile Movement => _movement;

    public SpriteSkin Skin => _skin;
}
