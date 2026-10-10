using System.Collections.Generic;
using UnityEngine;

public class LocationTable : ScriptableObject
{
    [SerializeField] private LocationRow[] _rows;

    public IReadOnlyList<LocationRow> Rows => _rows;

    public LocationRow GetRow(string id)
    {
        if (_rows == null)
            return null;

        foreach (LocationRow row in _rows)
        {
            if (row.Id == id)
                return row;
        }

        return null;
    }

    public LocationRow GetNext(LocationRow location)
    {
        if (_rows == null)
            return null;

        foreach (LocationRow row in _rows)
        {
            if (row.System == location.System && row.Order == location.Order + 1)
                return row;
        }

        return null;
    }

#if UNITY_EDITOR
    public void Fill(LocationRow[] rows)
    {
        _rows = rows;
    }
#endif
}
