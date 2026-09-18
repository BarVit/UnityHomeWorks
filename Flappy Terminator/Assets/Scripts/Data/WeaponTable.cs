using System.Collections.Generic;
using UnityEngine;

public class WeaponTable : ScriptableObject
{
    [SerializeField] private WeaponRow[] _rows;

    public IReadOnlyList<WeaponRow> Rows => _rows;

    public WeaponRow GetRow(string id)
    {
        if (_rows == null)
            return null;

        foreach (WeaponRow row in _rows)
        {
            if (row.Id == id)
                return row;
        }

        return null;
    }

#if UNITY_EDITOR
    public void Fill(WeaponRow[] rows)
    {
        _rows = rows;
    }
#endif
}
