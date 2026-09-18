using System;
using UnityEngine;

[Serializable]
public class EnemyShipRow
{
    [SerializeField] private string _id;
    [SerializeField] private string _title;
    [SerializeField] private EnemyShip _prefab;
    [SerializeField] private int _health;
    [SerializeField] private string _weapon;
    [SerializeField] private int _bodyDamage;
    [SerializeField] private string _fromLocation;

    public EnemyShipRow(string id, string title, EnemyShip prefab, int health, string weapon,
        int bodyDamage, string fromLocation)
    {
        _id = id;
        _title = title;
        _prefab = prefab;
        _health = health;
        _weapon = weapon;
        _bodyDamage = bodyDamage;
        _fromLocation = fromLocation;
    }

    public string Id => _id;

    public string Title => _title;

    public EnemyShip Prefab => _prefab;

    public int Health => _health;

    public string Weapon => _weapon;

    public int BodyDamage => _bodyDamage;

    public string FromLocation => _fromLocation;
}
