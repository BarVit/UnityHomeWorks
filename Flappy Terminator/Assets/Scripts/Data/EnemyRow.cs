using System;
using UnityEngine;

[Serializable]
public class EnemyRow
{
    [SerializeField] private string _id;
    [SerializeField] private string _type;
    [SerializeField] private string _title;
    [SerializeField] private int _tier;
    [SerializeField] private EnemyShip _prefab;
    [SerializeField] private int _bodyDamage;
    [SerializeField] private string _fromLocation;
    [SerializeField] private int _health;
    [SerializeField] private string _weapon;
    [SerializeField] private int _barrels;
    [SerializeField] private int _damage;
    [SerializeField] private MovementProfile _movement;
    [SerializeField] private SpriteSkin _skin;

    public EnemyRow(string id, string type, string title, int tier, EnemyShip prefab, int bodyDamage,
        string fromLocation, int health, string weapon, int barrels, int damage, MovementProfile movement,
        SpriteSkin skin)
    {
        _id = id;
        _type = type;
        _title = title;
        _tier = tier;
        _prefab = prefab;
        _bodyDamage = bodyDamage;
        _fromLocation = fromLocation;
        _health = health;
        _weapon = weapon;
        _barrels = barrels;
        _damage = damage;
        _movement = movement;
        _skin = skin;
    }

    public string Id => _id;

    public string Type => _type;

    public string Title => _title;

    public int Tier => _tier;

    public EnemyShip Prefab => _prefab;

    public int BodyDamage => _bodyDamage;

    public string FromLocation => _fromLocation;

    public int Health => _health;

    public string Weapon => _weapon;

    public int Barrels => _barrels;

    public int Damage => _damage;

    public MovementProfile Movement => _movement;

    public SpriteSkin Skin => _skin;
}
