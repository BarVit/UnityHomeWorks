using System;
using UnityEngine;

[Serializable]
public class SlotRow
{
    [SerializeField] private int _slot;
    [SerializeField] private int _price;
    [SerializeField] private string _weapon;
    [SerializeField] private string _gun;

    public SlotRow(string gun, int slot, int price, string weapon)
    {
        _gun = gun;
        _slot = slot;
        _price = price;
        _weapon = weapon;
    }

    public int Slot => _slot;

    public int Price => _price;

    public string Weapon => _weapon;

    public string Gun => _gun ?? AllGuns;

    public const string AllGuns = "";
}
