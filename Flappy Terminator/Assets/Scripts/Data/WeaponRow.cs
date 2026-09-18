using System;
using UnityEngine;

[Serializable]
public class WeaponRow
{
    [SerializeField] private string _id;
    [SerializeField] private DeliveryKind _delivery;
    [SerializeField] private int _damage;
    [SerializeField] private float _rate;
    [SerializeField] private float _projectileSpeed;
    [SerializeField] private int _barrelsMax;
    [SerializeField] private FirePattern _firePattern;
    [SerializeField] private float _heatPerShot;
    [SerializeField] private float _cooldownRate;
    [SerializeField] private int _price;
    [SerializeField] private Ammo _projectile;

    public WeaponRow(string id, DeliveryKind delivery, int damage, float rate, float projectileSpeed,
        int barrelsMax, FirePattern firePattern, float heatPerShot, float cooldownRate, int price, Ammo projectile)
    {
        _id = id;
        _delivery = delivery;
        _damage = damage;
        _rate = rate;
        _projectileSpeed = projectileSpeed;
        _barrelsMax = barrelsMax;
        _firePattern = firePattern;
        _heatPerShot = heatPerShot;
        _cooldownRate = cooldownRate;
        _price = price;
        _projectile = projectile;
    }

    public string Id => _id;

    public DeliveryKind Delivery => _delivery;

    public int Damage => _damage;

    public float Rate => _rate;

    public float ProjectileSpeed => _projectileSpeed;

    public int BarrelsMax => _barrelsMax;

    public FirePattern FirePattern => _firePattern;

    public float HeatPerShot => _heatPerShot;

    public float CooldownRate => _cooldownRate;

    public int Price => _price;

    public Ammo Projectile => _projectile;
}
