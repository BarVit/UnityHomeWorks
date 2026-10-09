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

    public int Health => _tier.Health;

    public int BodyDamage => _ship.BodyDamage;

    public int Barrels => _tier.Barrels;

    public int Damage => _tier.Damage;

    public MovementProfile Movement => _tier.Movement;

    public WeaponRow Weapon => _weapon;
}
