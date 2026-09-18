using System.Collections.Generic;
using UnityEngine;

public class EnemyShipTable : ScriptableObject
{
    [SerializeField] private EnemyShipRow[] _rows;

    public IReadOnlyList<EnemyShipRow> Rows => _rows;

    public EnemyShipRow GetRow(string id)
    {
        if (_rows == null)
            return null;

        foreach (EnemyShipRow row in _rows)
        {
            if (row.Id == id)
                return row;
        }

        return null;
    }

#if UNITY_EDITOR
    public void Fill(EnemyShipRow[] rows)
    {
        _rows = rows;
    }
#endif
}
