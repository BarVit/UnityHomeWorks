using UnityEngine;

public class EnemySetup
{
    private EnemySetup(EnemyShip prefab, string shipId, int tier, int health, int bodyDamage, int barrels,
        int damage, MovementProfile movement, WeaponRow weapon, int scale, Color tint, SpriteSkin skin)
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
        Skin = skin;
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

    public SpriteSkin Skin { get; private set; }

    public static EnemySetup ForEnemy(EnemyRow enemy, WeaponRow weapon)
    {
        return new EnemySetup(enemy.Prefab, enemy.Type, enemy.Tier, enemy.Health, enemy.BodyDamage, enemy.Barrels,
            enemy.Damage, enemy.Movement, weapon, 1, Color.white, enemy.Skin);
    }

    public static EnemySetup ForBoss(BossRow boss, WeaponRow weapon)
    {
        return new EnemySetup(boss.Prefab, boss.Id, 0, boss.Health, boss.BodyDamage, boss.Barrels,
            boss.Damage, boss.Movement, weapon, boss.Scale, boss.Tint, null);
    }
}
