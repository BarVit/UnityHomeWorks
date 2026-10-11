using System;
using UnityEngine;

[Serializable]
public class ModuleRow
{
    [SerializeField] private string _id;
    [SerializeField] private string _title;
    [SerializeField] private ModuleKind _kind;
    [SerializeField] private int _price;
    [SerializeField] private Sprite _icon;
    [SerializeField] private string _description;
    [SerializeField] private string _unlockAfter;

    public ModuleRow(string id, string title, ModuleKind kind, int price, Sprite icon, string description,
        string unlockAfter)
    {
        _unlockAfter = unlockAfter;
        _id = id;
        _title = title;
        _kind = kind;
        _price = price;
        _icon = icon;
        _description = description;
    }

    public string Id => _id;

    public string Title => _title;

    public ModuleKind Kind => _kind;

    public int Price => _price;

    public Sprite Icon => _icon;

    public string Description => _description;

    public string UnlockAfter => _unlockAfter;
}
