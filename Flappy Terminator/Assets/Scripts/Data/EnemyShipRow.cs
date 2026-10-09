using System;
using UnityEngine;

[Serializable]
public class EnemyShipRow
{
    [SerializeField] private string _id;
    [SerializeField] private string _title;
    [SerializeField] private EnemyShip _prefab;
    [SerializeField] private int _bodyDamage;
    [SerializeField] private string _fromLocation;

    public EnemyShipRow(string id, string title, EnemyShip prefab, int bodyDamage, string fromLocation)
    {
        _id = id;
        _title = title;
        _prefab = prefab;
        _bodyDamage = bodyDamage;
        _fromLocation = fromLocation;
    }

    public string Id => _id;

    public string Title => _title;

    public EnemyShip Prefab => _prefab;

    public int BodyDamage => _bodyDamage;

    public string FromLocation => _fromLocation;
}
