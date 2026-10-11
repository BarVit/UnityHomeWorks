using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class LevelTable<TStat> : ScriptableObject where TStat : struct, Enum
{
    public const string Everyone = "";

    [SerializeField] private LevelRow<TStat>[] _rows;

    public IReadOnlyList<LevelRow<TStat>> Rows => _rows;

    public LevelRow<TStat> GetRow(string owner, TStat stat, int level)
    {
        if (_rows == null)
            return null;

        owner = Resolve(owner, stat);

        foreach (LevelRow<TStat> row in _rows)
        {
            if (Matches(row, owner, stat) && row.Level == level)
                return row;
        }

        return null;
    }

    public int GetMaxLevel(string owner, TStat stat)
    {
        int max = 0;

        if (_rows == null)
            return max;

        owner = Resolve(owner, stat);

        foreach (LevelRow<TStat> row in _rows)
        {
            if (Matches(row, owner, stat) && row.Level > max)
                max = row.Level;
        }

        return max;
    }

    public float GetValue(string owner, TStat stat, int level)
    {
        float value = 0f;

        if (_rows == null)
            return value;

        owner = Resolve(owner, stat);

        foreach (LevelRow<TStat> row in _rows)
        {
            if (Matches(row, owner, stat) && row.Level <= level)
                value += row.Bonus;
        }

        return value;
    }

    public float GetStep(string owner, TStat stat)
    {
        return GetValue(owner, stat, 1) - GetValue(owner, stat, 0);
    }

    public List<TStat> GetUpgradableStats(string owner)
    {
        List<TStat> stats = new List<TStat>();

        foreach (TStat stat in (TStat[])Enum.GetValues(typeof(TStat)))
        {
            if (GetMaxLevel(owner, stat) > 0)
                stats.Add(stat);
        }

        return stats;
    }

    private string Resolve(string owner, TStat stat)
    {
        foreach (LevelRow<TStat> row in _rows)
        {
            if (row.Owner == owner && EqualityComparer<TStat>.Default.Equals(row.Stat, stat))
                return owner;
        }

        return Everyone;
    }

    private static bool Matches(LevelRow<TStat> row, string owner, TStat stat)
    {
        return EqualityComparer<TStat>.Default.Equals(row.Stat, stat) && row.Owner == owner;
    }

#if UNITY_EDITOR
    public void Fill(LevelRow<TStat>[] rows)
    {
        _rows = rows;
    }
#endif
}
