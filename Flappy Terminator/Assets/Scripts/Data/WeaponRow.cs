using System;
using UnityEngine;

[Serializable]
public class WeaponRow
{
    [SerializeField] private string _id;
    [SerializeField] private WeaponOwner _owner;
    [SerializeField] private DeliveryKind _delivery;
    [SerializeField] private int _damage;
    [SerializeField] private float _rate;
    [SerializeField] private float _projectileSpeed;
    [SerializeField] private int _barrelsMax;
    [SerializeField] private FirePattern _firePattern;
    [SerializeField] private float _heatPerShot;
    [SerializeField] private float _cooldownRate;
    [SerializeField] private float _tickInterval;
    [SerializeField] private float _turnRate;
    [SerializeField] private float _area;
    [SerializeField] private int _price;
    [SerializeField] private Ammo _projectile;
    [SerializeField] private BeamFlash _beam;
    [SerializeField] private WeaponAssets _assets;

    public WeaponRow(string id, WeaponOwner owner, DeliveryKind delivery, int damage, float rate,
        float projectileSpeed, int barrelsMax, FirePattern firePattern, float heatPerShot,
        float cooldownRate, float tickInterval, float turnRate, float area, int price,
        Ammo projectile, BeamFlash beam, WeaponAssets assets)
    {
        _id = id;
        _owner = owner;
        _delivery = delivery;
        _damage = damage;
        _rate = rate;
        _projectileSpeed = projectileSpeed;
        _barrelsMax = barrelsMax;
        _firePattern = firePattern;
        _heatPerShot = heatPerShot;
        _cooldownRate = cooldownRate;
        _tickInterval = tickInterval;
        _turnRate = turnRate;
        _area = area;
        _price = price;
        _projectile = projectile;
        _beam = beam;
        _assets = assets;
    }

    public string Id => _id;

    public WeaponOwner Owner => _owner;

    public DeliveryKind Delivery => _delivery;

    public int Damage => _damage;

    public float Rate => _rate;

    public float ProjectileSpeed => _projectileSpeed;

    public int BarrelsMax => _barrelsMax;

    public FirePattern FirePattern => _firePattern;

    public float HeatPerShot => _heatPerShot;

    public float CooldownRate => _cooldownRate;

    public float TickInterval => _tickInterval;

    public float TurnRate => _turnRate;

    public float Area => _area;

    public int Price => _price;

    public Ammo Projectile => _projectile;

    public BeamFlash Beam => _beam;

    public WeaponAssets Assets => _assets;

    public ExplosionAnimation HitEffect => _assets != null && _assets.HitTargetEffect != null
        ? _assets.HitTargetEffect
        : (_projectile == null ? null : _projectile.HitEffectPrefab);
}
