using System.Collections.Generic;
using UnityEngine;

public class ModuleTable : ScriptableObject
{
    [SerializeField] private ModuleRow[] _rows;

    public IReadOnlyList<ModuleRow> Rows => _rows;

    public ModuleRow GetRow(string id)
    {
        if (_rows == null)
            return null;

        foreach (ModuleRow row in _rows)
        {
            if (row.Id == id)
                return row;
        }

        return null;
    }

#if UNITY_EDITOR
    public void Fill(ModuleRow[] rows)
    {
        _rows = rows;
    }
#endif
}
