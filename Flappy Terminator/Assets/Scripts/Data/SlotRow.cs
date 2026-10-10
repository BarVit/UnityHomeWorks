using System;
using UnityEngine;

[Serializable]
public class SlotRow
{
    [SerializeField] private int _slot;
    [SerializeField] private int _price;
    [SerializeField] private string _weapon;

    public SlotRow(int slot, int price, string weapon)
    {
        _slot = slot;
        _price = price;
        _weapon = weapon;
    }

    public int Slot => _slot;

    public int Price => _price;

    public string Weapon => _weapon;
}
