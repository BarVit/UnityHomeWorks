using System.Collections.Generic;
using UnityEngine;

public class EnemyTable : ScriptableObject
{
    [SerializeField] private EnemyRow[] _rows;

    public IReadOnlyList<EnemyRow> Rows => _rows;

    public EnemyRow GetRow(string id)
    {
        if (_rows == null)
            return null;

        foreach (EnemyRow row in _rows)
        {
            if (row.Id == id)
                return row;
        }

        return null;
    }

#if UNITY_EDITOR
    public void Fill(EnemyRow[] rows)
    {
        _rows = rows;
    }
#endif
}
