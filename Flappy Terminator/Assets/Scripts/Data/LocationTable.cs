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

        if (location.IsBranch)
        {
            LocationRow parent = GetRow(location.BranchOf);

            return parent == null ? null : GetNext(parent);
        }

        foreach (LocationRow row in _rows)
        {
            if (row.IsBranch == false && row.System == location.System && row.Order == location.Order + 1)
                return row;
        }

        return null;
    }

    public bool IsVoid(string voidId)
    {
        return GetVoid(voidId).Count > 0;
    }

    public List<LocationRow> GetVoid(string voidId)
    {
        List<LocationRow> found = new List<LocationRow>();

        if (_rows == null || string.IsNullOrEmpty(voidId))
            return found;

        foreach (LocationRow row in _rows)
        {
            if (row.Void == voidId)
                found.Add(row);
        }

        found.Sort((left, right) => left.Order.CompareTo(right.Order));

        return found;
    }

    public List<LocationRow> GetBranches(string parentId)
    {
        List<LocationRow> branches = new List<LocationRow>();

        if (_rows == null)
            return branches;

        foreach (LocationRow row in _rows)
        {
            if (row.BranchOf == parentId)
                branches.Add(row);
        }

        return branches;
    }

#if UNITY_EDITOR
    public void Fill(LocationRow[] rows)
    {
        _rows = rows;
    }
#endif
}
