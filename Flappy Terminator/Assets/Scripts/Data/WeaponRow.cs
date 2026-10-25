using System;
using UnityEngine;

[Serializable]
public class WeaponRow
{
    [SerializeField] private string _id;
    [SerializeField] private string _title;
    [SerializeField] private WeaponOwner _owner;
    [SerializeField] private DeliveryKind _delivery;
    [SerializeField] private int _damage;
    [SerializeField] private float _rate;
    [SerializeField] private int _magazine;
    [SerializeField] private float _reloadTime;
    [SerializeField] private float _projectileSpeed;
    [SerializeField] private int _barrelsMax;
    [SerializeField] private FirePattern _firePattern;
    [SerializeField] private float _sideAngle;
    [SerializeField] private float _heatPerShot;
    [SerializeField] private float _cooldownRate;
    [SerializeField] private float _unlockHeat;
    [SerializeField] private float _tickInterval;
    [SerializeField] private float _turnRate;
    [SerializeField] private float _aimTime;
    [SerializeField] private float _area;
    [SerializeField] private BlastSettings _blast;
    [SerializeField] private int _price;
    [SerializeField] private string _unlockAfter;
    [SerializeField] private string _grantedAfter;
    [SerializeField] private Ammo _projectile;
    [SerializeField] private BeamFlash _beam;
    [SerializeField] private WeaponAssets _assets;

    public WeaponRow(string id, string title, WeaponOwner owner, DeliveryKind delivery, int damage, float rate,
        int magazine, float reloadTime, float projectileSpeed, int barrelsMax, FirePattern firePattern, float sideAngle, float heatPerShot,
        float cooldownRate, float unlockHeat, float tickInterval, float turnRate, float aimTime, float area, BlastSettings blast, int price,
        string unlockAfter, string grantedAfter, Ammo projectile, BeamFlash beam, WeaponAssets assets)
    {
        _id = id;
        _title = title;
        _owner = owner;
        _delivery = delivery;
        _damage = damage;
        _rate = rate;
        _magazine = magazine;
        _reloadTime = reloadTime;
        _projectileSpeed = projectileSpeed;
        _barrelsMax = barrelsMax;
        _firePattern = firePattern;
        _sideAngle = sideAngle;
        _heatPerShot = heatPerShot;
        _cooldownRate = cooldownRate;
        _unlockHeat = unlockHeat;
        _tickInterval = tickInterval;
        _turnRate = turnRate;
        _aimTime = aimTime;
        _area = area;
        _blast = blast;
        _price = price;
        _unlockAfter = unlockAfter;
        _grantedAfter = grantedAfter;
        _projectile = projectile;
        _beam = beam;
        _assets = assets;
    }

    public string Id => _id;

    public string Title => _title;

    public WeaponOwner Owner => _owner;

    public DeliveryKind Delivery => _delivery;

    public int Damage => _damage;

    public float Rate => _rate;

    public int Magazine => _magazine;

    public float ReloadTime => _reloadTime;

    public float ProjectileSpeed => _projectileSpeed;

    public int BarrelsMax => _barrelsMax;

    public FirePattern FirePattern => _firePattern;

    public float SideAngle => _sideAngle;

    public float HeatPerShot => _heatPerShot;

    public float CooldownRate => _cooldownRate;

    public float UnlockHeat => _unlockHeat;

    public float TickInterval => _tickInterval;

    public float TurnRate => _turnRate;

    public float AimTime => _aimTime;

    public float Area => _area;

    public BlastSettings Blast => _blast;

    public int Price => _price;

    public string UnlockAfter => _unlockAfter;

    public string GrantedAfter => _grantedAfter;

    public Ammo Projectile => _projectile;

    public BeamFlash Beam => _beam;

    public WeaponAssets Assets => _assets;

    public ExplosionAnimation HitEffect => _assets != null && _assets.HitTargetEffect != null
        ? _assets.HitTargetEffect
        : (_projectile == null ? null : _projectile.HitEffectPrefab);
}
