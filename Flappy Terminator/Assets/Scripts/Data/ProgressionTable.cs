using System.Collections.Generic;
using UnityEngine;

public class ProgressionTable : ScriptableObject
{
    [SerializeField] private ProgressionRow[] _rows;

    public IReadOnlyList<ProgressionRow> Rows => _rows;

    public ProgressionRow GetRowByLevel(int level)
    {
        if (_rows == null || _rows.Length == 0)
            return null;

        ProgressionRow best = null;

        foreach (ProgressionRow row in _rows)
        {
            if (row.PilotLevel > level)
                continue;

            if (best == null || row.PilotLevel > best.PilotLevel)
                best = row;
        }

        return best ?? _rows[0];
    }

    public ProgressionRow GetRow(string id)
    {
        if (_rows == null)
            return null;

        foreach (ProgressionRow row in _rows)
        {
            if (row.Id == id)
                return row;
        }

        return null;
    }

#if UNITY_EDITOR
    public void Fill(ProgressionRow[] rows)
    {
        _rows = rows;
    }
#endif
}
