using System;
using UnityEngine;

[Serializable]
public class BossRow
{
    [SerializeField] private string _id;
    [SerializeField] private string _title;
    [SerializeField] private BossKind _kind;
    [SerializeField] private EnemyShip _prefab;
    [SerializeField] private string _prototype;
    [SerializeField] private int _health;
    [SerializeField] private string _weapon;
    [SerializeField] private int _barrels;
    [SerializeField] private int _damage;
    [SerializeField] private int _bodyDamage;
    [SerializeField] private int _scale;
    [SerializeField] private Color _tint;
    [SerializeField] private MovementProfile _movement;

    public BossRow(string id, string title, BossKind kind, EnemyShip prefab, string prototype, int health,
        string weapon, int barrels, int damage, int bodyDamage, int scale, Color tint, MovementProfile movement)
    {
        _id = id;
        _title = title;
        _kind = kind;
        _prefab = prefab;
        _prototype = prototype;
        _health = health;
        _weapon = weapon;
        _barrels = barrels;
        _damage = damage;
        _bodyDamage = bodyDamage;
        _scale = scale;
        _tint = tint;
        _movement = movement;
    }

    public string Id => _id;

    public string Title => _title;

    public BossKind Kind => _kind;

    public EnemyShip Prefab => _prefab;

    public string Prototype => _prototype;

    public int Health => _health;

    public string Weapon => _weapon;

    public int Barrels => _barrels;

    public int Damage => _damage;

    public int BodyDamage => _bodyDamage;

    public int Scale => _scale;

    public Color Tint => _tint;

    public MovementProfile Movement => _movement;
}
