using System.Collections.Generic;
using UnityEngine;
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

        int batchSize = 0;
        int batchLeft = 0;
        int slotShift = 0;

        for (int i = 0; i < row.Count; i++)
        {
            if (batchLeft == 0)
            {
                batchSize = Mathf.Min(Random.Range(row.BatchMin, row.BatchMax + 1), row.Count - i);
                batchLeft = batchSize;
                slotShift = Random.Range(0, batchSize);
            }

            string enemyId = row.Mode == WaveMode.Sequence
                ? row.Enemies[i % row.Enemies.Length]
                : row.Enemies[Random.Range(0, row.Enemies.Length)];
            int slot = (batchSize - batchLeft + slotShift) % batchSize;

            batchLeft--;

            float interval = batchLeft > 0 ? 0f : GetInterval(row);

            plan.Add(new PlannedSpawn(CreateEnemy(enemyId, row), interval, false, PlannedSpawn.NoAnnouncement, slot,
                batchSize));
        }
    }

    private float GetInterval(WaveRow row)
    {
        return row.IntervalMax > row.Interval ? Random.Range(row.Interval, row.IntervalMax) : row.Interval;
    }

    private EnemySetup CreateEnemy(string enemyId, WaveRow row)
    {
        EnemyRow enemy = _enemies.GetRow(enemyId);

        return EnemySetup.ForEnemy(enemy, _weapons.GetRow(enemy.Weapon), row.ShieldCapacity, row.ShieldReward,
            row.ShieldLevel);
    }
}
