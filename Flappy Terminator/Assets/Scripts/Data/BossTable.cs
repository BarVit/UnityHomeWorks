using System.Collections.Generic;
using UnityEngine;

public class BossTable : ScriptableObject
{
    [SerializeField] private BossRow[] _rows;

    public IReadOnlyList<BossRow> Rows => _rows;

    public BossRow GetRow(string id)
    {
        if (_rows == null)
            return null;

        foreach (BossRow row in _rows)
        {
            if (row.Id == id)
                return row;
        }

        return null;
    }

#if UNITY_EDITOR
    public void Fill(BossRow[] rows)
    {
        _rows = rows;
    }
#endif
}
