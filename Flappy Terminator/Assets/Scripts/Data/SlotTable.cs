using System.Collections.Generic;
using UnityEngine;

public class SlotTable : ScriptableObject
{
    [SerializeField] private SlotRow[] _rows;

    public IReadOnlyList<SlotRow> Rows => _rows;

    public int Count => _rows == null ? 0 : _rows.Length;

    public SlotRow GetRow(int slot)
    {
        if (_rows == null)
            return null;

        foreach (SlotRow row in _rows)
        {
            if (row.Slot == slot)
                return row;
        }

        return null;
    }

#if UNITY_EDITOR
    public void Fill(SlotRow[] rows)
    {
        _rows = rows;
    }
#endif
}
