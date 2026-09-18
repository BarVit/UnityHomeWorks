using System.Collections.Generic;
using UnityEngine;

public class EnemyTierTable : ScriptableObject
{
    [SerializeField] private EnemyTierRow[] _rows;

    public IReadOnlyList<EnemyTierRow> Rows => _rows;

    public EnemyTierRow GetRow(string id)
    {
        if (_rows == null)
            return null;

        foreach (EnemyTierRow row in _rows)
        {
            if (row.Id == id)
                return row;
        }

        return null;
    }

    public List<EnemyTierRow> GetTiers(string shipId)
    {
        List<EnemyTierRow> found = new List<EnemyTierRow>();

        if (_rows == null)
            return found;

        foreach (EnemyTierRow row in _rows)
        {
            if (row.Ship == shipId)
                found.Add(row);
        }

        return found;
    }

#if UNITY_EDITOR
    public void Fill(EnemyTierRow[] rows)
    {
        _rows = rows;
    }
#endif
}
