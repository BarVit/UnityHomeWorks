using System.Collections.Generic;
using Random = UnityEngine.Random;

public class LevelPlanner
{
    private readonly EnemyTable _enemies;
    private readonly WeaponTable _weapons;
    private readonly BossTable _bosses;
    private readonly WaveTable _waves;

    public LevelPlanner(EnemyTable enemies, WeaponTable weapons, BossTable bosses, WaveTable waves)
    {
        _enemies = enemies;
        _weapons = weapons;
        _bosses = bosses;
        _waves = waves;
    }

    public List<PlannedSpawn> Plan(string locationId, int level)
    {
        List<PlannedSpawn> plan = new List<PlannedSpawn>();

        foreach (WaveRow row in _waves.GetLevel(locationId, level))
            AddGroup(plan, row);

        return plan;
    }

    private void AddGroup(List<PlannedSpawn> plan, WaveRow row)
    {
        if (row.Mode == WaveMode.Boss)
        {
            BossRow boss = _bosses.GetRow(row.Enemies[0]);

            plan.Add(new PlannedSpawn(EnemySetup.ForBoss(boss, _weapons), GetInterval(row), true, boss.Intro));

            return;
        }

        for (int i = 0; i < row.Count; i++)
        {
            string enemyId = row.Mode == WaveMode.Sequence
                ? row.Enemies[i % row.Enemies.Length]
                : row.Enemies[Random.Range(0, row.Enemies.Length)];

            plan.Add(new PlannedSpawn(CreateEnemy(enemyId), GetInterval(row), false, PlannedSpawn.NoAnnouncement));
        }
    }

    private float GetInterval(WaveRow row)
    {
        return row.IntervalMax > row.Interval ? Random.Range(row.Interval, row.IntervalMax) : row.Interval;
    }

    private EnemySetup CreateEnemy(string enemyId)
    {
        EnemyRow enemy = _enemies.GetRow(enemyId);

        return EnemySetup.ForEnemy(enemy, _weapons.GetRow(enemy.Weapon));
    }
}
