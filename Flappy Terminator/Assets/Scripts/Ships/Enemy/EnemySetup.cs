using UnityEngine;

public class EnemySetup
{
    private readonly EnemyShipRow _ship;
    private readonly EnemyTierRow _tier;
    private readonly WeaponRow _weapon;

    public EnemySetup(EnemyShipRow ship, EnemyTierRow tier, WeaponRow weapon)
    {
        _ship = ship;
        _tier = tier;
        _weapon = weapon;
    }

    public EnemyShip Prefab => _ship.Prefab;

    public string ShipId => _ship.Id;

    public int Tier => _tier.Tier;

    public int Health => Mathf.Max(1, Mathf.RoundToInt(_ship.Health * _tier.HealthScale));

    public int BodyDamage => _ship.BodyDamage;

    public int Barrels => _tier.Barrels;

    public float ProjectileSpeedScale => _tier.ProjectileSpeedScale;

    public MovementProfile Movement => _tier.Movement;

    public WeaponRow Weapon => _weapon;
}
