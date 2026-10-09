using UnityEngine;

public class EnemySetup
{
    private EnemySetup(EnemyShip prefab, string shipId, int tier, int health, int bodyDamage, int barrels,
        int damage, MovementProfile movement, WeaponRow weapon, int scale, Color tint)
    {
        Prefab = prefab;
        ShipId = shipId;
        Tier = tier;
        Health = health;
        BodyDamage = bodyDamage;
        Barrels = barrels;
        Damage = damage;
        Movement = movement;
        Weapon = weapon;
        Scale = scale;
        Tint = tint;
    }

    public EnemyShip Prefab { get; private set; }

    public string ShipId { get; private set; }

    public int Tier { get; private set; }

    public int Health { get; private set; }

    public int BodyDamage { get; private set; }

    public int Barrels { get; private set; }

    public int Damage { get; private set; }

    public MovementProfile Movement { get; private set; }

    public WeaponRow Weapon { get; private set; }

    public int Scale { get; private set; }

    public Color Tint { get; private set; }

    public static EnemySetup ForTier(EnemyShipRow ship, EnemyTierRow tier, WeaponRow weapon)
    {
        return new EnemySetup(ship.Prefab, ship.Id, tier.Tier, tier.Health, ship.BodyDamage, tier.Barrels,
            tier.Damage, tier.Movement, weapon, 1, Color.white);
    }

    public static EnemySetup ForBoss(BossRow boss, WeaponRow weapon)
    {
        return new EnemySetup(boss.Prefab, boss.Id, 0, boss.Health, boss.BodyDamage, boss.Barrels,
            boss.Damage, boss.Movement, weapon, boss.Scale, boss.Tint);
    }
}
