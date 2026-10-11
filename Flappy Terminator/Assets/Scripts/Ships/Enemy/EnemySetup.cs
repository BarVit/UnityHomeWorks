using System.Collections.Generic;
using UnityEngine;

public class EnemySetup
{
    private static readonly BossPhase[] NoPhases = new BossPhase[0];

    private EnemySetup(EnemyShip prefab, string shipId, int tier, int health, int bodyDamage,
        EnemyArmament armament, MovementProfile movement, int scale, Color tint, SpriteSkin skin,
        int rewardMoney, int rewardXp, IReadOnlyList<BossPhase> phases, EnemyArmament special = null,
        int specialEvery = 0)
    {
        Special = special;
        SpecialEvery = specialEvery;
        Prefab = prefab;
        ShipId = shipId;
        Tier = tier;
        Health = health;
        BodyDamage = bodyDamage;
        Armament = armament;
        Movement = movement;
        Scale = scale;
        Tint = tint;
        Skin = skin;
        RewardMoney = rewardMoney;
        RewardXp = rewardXp;
        Phases = phases;
    }

    public EnemyShip Prefab { get; private set; }

    public string ShipId { get; private set; }

    public int Tier { get; private set; }

    public int Health { get; private set; }

    public int BodyDamage { get; private set; }

    public MovementProfile Movement { get; private set; }

    public EnemyArmament Armament { get; private set; }

    public int Scale { get; private set; }

    public Color Tint { get; private set; }

    public SpriteSkin Skin { get; private set; }

    public int RewardMoney { get; private set; }

    public int RewardXp { get; private set; }

    public IReadOnlyList<BossPhase> Phases { get; private set; }

    public EnemyArmament Special { get; private set; }

    public int SpecialEvery { get; private set; }

    public static EnemySetup ForEnemy(EnemyRow enemy, WeaponRow weapon)
    {
        return new EnemySetup(enemy.Prefab, enemy.Type, enemy.Tier, enemy.Health, enemy.BodyDamage,
            new EnemyArmament(weapon, enemy.Barrels, enemy.Damage, 1f), enemy.Movement, 1, Color.white, enemy.Skin,
            enemy.RewardMoney, enemy.RewardXp, NoPhases);
    }

    public static EnemySetup ForBoss(BossRow boss, WeaponTable weapons)
    {
        return new EnemySetup(boss.Prefab, boss.Id, 0, boss.Health, boss.BodyDamage,
            new EnemyArmament(weapons.GetRow(boss.Weapon), boss.Barrels, boss.Damage, 1f), boss.Movement,
            boss.Scale, boss.Tint, null, boss.RewardMoney, boss.RewardXp,
            CreatePhases(boss.Phases, weapons), CreateSpecial(boss, weapons), boss.SpecialEvery);
    }

    private static EnemyArmament CreateSpecial(BossRow boss, WeaponTable weapons)
    {
        if (boss.SpecialEvery <= 0)
            return null;

        return new EnemyArmament(weapons.GetRow(boss.SpecialWeapon), boss.SpecialBarrels, boss.Damage, 1f);
    }

    private static List<BossPhase> CreatePhases(IReadOnlyList<BossPhaseRow> rows, WeaponTable weapons)
    {
        List<BossPhase> phases = new List<BossPhase>();

        for (int i = 0; i < rows.Count; i++)
        {
            BossPhaseRow row = rows[i];
            EnemyArmament armament = new EnemyArmament(weapons.GetRow(row.Weapon), row.Barrels, row.Damage,
                row.RateScale);

            phases.Add(new BossPhase(armament, row.Exit, row.ExitValue, GetNextPhase(row, i, rows.Count),
                row.EscapeHealth, row.EscapeTo - 1));
        }

        return phases;
    }

    private static int GetNextPhase(BossPhaseRow row, int index, int count)
    {
        if (row.Next != BossPhaseRow.FollowingPhase)
            return row.Next - 1;

        return index + 1 < count ? index + 1 : BossPhase.Stay;
    }
}
