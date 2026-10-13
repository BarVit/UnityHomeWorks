using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BossRow
{
    [SerializeField] private string _id;
    [SerializeField] private string _title;
    [SerializeField] private BossKind _kind;
    [SerializeField] private string _location;
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
    [SerializeField] private int _rewardMoney;
    [SerializeField] private int _rewardXp;
    [SerializeField] private string _intro;
    [SerializeField] private int _specialEvery;
    [SerializeField] private string _specialWeapon;
    [SerializeField] private int _specialBarrels;
    [SerializeField] private BossPhaseRow[] _phases;

    public BossRow(string id, string title, BossKind kind, string location, EnemyShip prefab, string prototype,
        int health, string weapon, int barrels, int damage, int bodyDamage, int scale, Color tint,
        MovementProfile movement, int rewardMoney, int rewardXp, string intro, int specialEvery, string specialWeapon,
        int specialBarrels, BossPhaseRow[] phases)
    {
        _specialEvery = specialEvery;
        _specialWeapon = specialWeapon;
        _specialBarrels = specialBarrels;
        _id = id;
        _title = title;
        _kind = kind;
        _location = location;
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
        _rewardMoney = rewardMoney;
        _rewardXp = rewardXp;
        _intro = intro;
        _phases = phases;
    }

    public string Id => _id;

    public string Title => _title;

    public BossKind Kind => _kind;

    public string Location => _location;

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

    public int RewardMoney => _rewardMoney;

    public int RewardXp => _rewardXp;

    public string Intro => _intro;

    public int SpecialEvery => _specialEvery;

    public string SpecialWeapon => _specialWeapon;

    public int SpecialBarrels => _specialBarrels;

    public IReadOnlyList<BossPhaseRow> Phases => _phases ?? Array.Empty<BossPhaseRow>();
}
