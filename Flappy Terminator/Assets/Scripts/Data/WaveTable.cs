using System.Collections.Generic;
using UnityEngine;

public class WaveTable : ScriptableObject
{
    [SerializeField] private WaveRow[] _rows;

    public IReadOnlyList<WaveRow> Rows => _rows;

    public List<WaveRow> GetLevel(string location, int level)
    {
        List<WaveRow> found = new List<WaveRow>();

        if (_rows == null)
            return found;

        foreach (WaveRow row in _rows)
        {
            if (row.Location == location && row.Level == level)
                found.Add(row);
        }

        found.Sort((left, right) => left.Order.CompareTo(right.Order));

        return found;
    }

    public bool HasLocation(string location)
    {
        if (_rows == null)
            return false;

        foreach (WaveRow row in _rows)
        {
            if (row.Location == location)
                return true;
        }

        return false;
    }

    public bool IsPlayable(LocationRow location)
    {
        for (int level = 1; level <= location.Levels; level++)
        {
            if (GetLevel(location.Id, level).Count == 0)
                return false;
        }

        return true;
    }

#if UNITY_EDITOR
    public void Fill(WaveRow[] rows)
    {
        _rows = rows;
    }
#endif
}
