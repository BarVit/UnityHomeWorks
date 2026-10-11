using System.Collections.Generic;
using UnityEngine;

public class SlotTable : ScriptableObject
{
    [SerializeField] private SlotRow[] _rows;

    public IReadOnlyList<SlotRow> Rows => _rows;

    public int Count
    {
        get
        {
            int count = 0;

            if (_rows == null)
                return count;

            foreach (SlotRow row in _rows)
            {
                if (row.Gun == SlotRow.AllGuns)
                    count++;
            }

            return count;
        }
    }

    public SlotRow GetRow(int slot)
    {
        return Find(SlotRow.AllGuns, slot);
    }

    public SlotRow GetRow(string gun, int slot)
    {
        return Find(gun, slot) ?? Find(SlotRow.AllGuns, slot);
    }

    private SlotRow Find(string gun, int slot)
    {
        if (_rows == null)
            return null;

        foreach (SlotRow row in _rows)
        {
            if (row.Gun == gun && row.Slot == slot)
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
