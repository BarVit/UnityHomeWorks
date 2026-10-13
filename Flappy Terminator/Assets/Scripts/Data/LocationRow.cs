using System;
using UnityEngine;

[Serializable]
public class LocationRow
{
    [SerializeField] private string _id;
    [SerializeField] private string _system;
    [SerializeField] private int _order;
    [SerializeField] private int _levels;
    [SerializeField] private string _borderTheme;
    [SerializeField] private FlightMode _flightMode;
    [SerializeField] private string _title;
    [SerializeField] private string _branchOf;
    [SerializeField] private string _void;

    public LocationRow(string id, string system, int order, int levels, string borderTheme,
        FlightMode flightMode, string title, string branchOf, string voidId)
    {
        _void = voidId;
        _id = id;
        _system = system;
        _order = order;
        _levels = levels;
        _borderTheme = borderTheme;
        _flightMode = flightMode;
        _title = title;
        _branchOf = branchOf;
    }

    public string Id => _id;

    public string System => _system;

    public int Order => _order;

    public int Levels => _levels;

    public string BorderTheme => _borderTheme;

    public FlightMode FlightMode => _flightMode;

    public string Title => _title;

    public string BranchOf => _branchOf;

    public bool IsBranch => string.IsNullOrEmpty(_branchOf) == false;

    public string Void => _void;

    public bool IsInVoid => string.IsNullOrEmpty(_void) == false;
}
