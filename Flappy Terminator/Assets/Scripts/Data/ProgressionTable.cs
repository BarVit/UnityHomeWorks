using System.Collections.Generic;
using UnityEngine;

public class ProgressionTable : ScriptableObject
{
    [SerializeField] private ProgressionRow[] _rows;

    public IReadOnlyList<ProgressionRow> Rows => _rows;

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
