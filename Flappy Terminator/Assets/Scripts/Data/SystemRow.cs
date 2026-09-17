using System;
using UnityEngine;

[Serializable]
public class SystemRow
{
    [SerializeField] private string _id;
    [SerializeField] private string _title;
    [SerializeField] private int _order;

    public SystemRow(string id, string title, int order)
    {
        _id = id;
        _title = title;
        _order = order;
    }

    public string Id => _id;

    public string Title => _title;

    public int Order => _order;
}
