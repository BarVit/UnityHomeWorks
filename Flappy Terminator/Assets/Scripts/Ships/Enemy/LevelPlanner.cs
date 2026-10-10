using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class LevelPlanner
{
    private readonly EnemyTable _enemies;
    private readonly WeaponTable _weapons;
    private readonly BossTable _bosses;
    private readonly WaveTable _waves;
    private readonly EnemyRoster _roster;

    public LevelPlanner(EnemyTable enemies, WeaponTable weapons, BossTable bosses, WaveTable waves,
        EnemyRoster roster)
    {
        _enemies = enemies;
        _weapons = weapons;
        _bosses = bosses;
        _waves = waves;
        _roster = roster;
    }

    public List<PlannedSpawn> Plan(string locationId, int level, int fallbackSize, float fallbackInterval)
    {
        List<WaveRow> rows = _waves == null ? new List<WaveRow>() : _waves.GetLevel(locationId, level);
        List<PlannedSpawn> plan = new List<PlannedSpawn>();

        if (rows.Count == 0)
        {
            AddRandom(plan, fallbackSize, fallbackInterval);

            return plan;
        }

        foreach (WaveRow row in rows)
            AddGroup(plan, row);

        return plan;
    }

    private void AddRandom(List<PlannedSpawn> plan, int size, float interval)
    {
        if (_roster.Available.Count == 0)
            throw new InvalidOperationException("LevelPlanner: для локации нет ни одного открытого врага");

        for (int i = 0; i < size; i++)
            plan.Add(new PlannedSpawn(_roster.GetRandom(), interval, false));
    }

    private void AddGroup(List<PlannedSpawn> plan, WaveRow row)
    {
        if (row.Mode == WaveMode.Boss)
        {
            BossRow boss = _bosses.GetRow(row.Enemies[0]);

            plan.Add(new PlannedSpawn(EnemySetup.ForBoss(boss, _weapons.GetRow(boss.Weapon)), GetInterval(row), true));

            return;
        }

        for (int i = 0; i < row.Count; i++)
        {
            string enemyId = row.Mode == WaveMode.Sequence
                ? row.Enemies[i % row.Enemies.Length]
                : row.Enemies[Random.Range(0, row.Enemies.Length)];

            plan.Add(new PlannedSpawn(CreateEnemy(enemyId), GetInterval(row), false));
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
