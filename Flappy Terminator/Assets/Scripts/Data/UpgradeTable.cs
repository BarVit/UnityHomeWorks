using System.Collections.Generic;
using UnityEngine;

public class UpgradeTable : ScriptableObject
{
    [SerializeField] private UpgradeRow[] _rows;

    public IReadOnlyList<UpgradeRow> Rows => _rows;

    public UpgradeRow GetRow(UpgradeStat stat, int level)
    {
        if (_rows == null)
            return null;

        foreach (UpgradeRow row in _rows)
        {
            if (row.Stat == stat && row.Level == level)
                return row;
        }

        return null;
    }

    public int GetMaxLevel(UpgradeStat stat)
    {
        int max = 0;

        if (_rows == null)
            return max;

        foreach (UpgradeRow row in _rows)
        {
            if (row.Stat == stat && row.Level > max)
                max = row.Level;
        }

        return max;
    }

    public float GetValue(UpgradeStat stat, int level)
    {
        float value = 0f;

        if (_rows == null)
            return value;

        foreach (UpgradeRow row in _rows)
        {
            if (row.Stat == stat && row.Level <= level)
                value += row.Bonus;
        }

        return value;
    }

#if UNITY_EDITOR
    public void Fill(UpgradeRow[] rows)
    {
        _rows = rows;
    }
#endif
}
