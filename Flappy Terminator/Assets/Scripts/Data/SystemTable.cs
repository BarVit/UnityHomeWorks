using System.Collections.Generic;
using UnityEngine;

public class SystemTable : ScriptableObject
{
    [SerializeField] private SystemRow[] _rows;

    public IReadOnlyList<SystemRow> Rows => _rows;

    public SystemRow GetRow(string id)
    {
        if (_rows == null)
            return null;

        foreach (SystemRow row in _rows)
        {
            if (row.Id == id)
                return row;
        }

        return null;
    }

#if UNITY_EDITOR
    public void Fill(SystemRow[] rows)
    {
        _rows = rows;
    }
#endif
}
