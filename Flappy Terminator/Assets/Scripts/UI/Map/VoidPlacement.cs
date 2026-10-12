using System;
using UnityEngine;

[Serializable]
public class VoidPlacement
{
    [SerializeField] private string _id;
    [SerializeField] private string _title;
    [SerializeField] private Vector2 _system;
    [SerializeField] private Vector2 _arm;
    [SerializeField] private Vector2[] _locations;

    public VoidPlacement(string id, string title, Vector2 system, Vector2 arm, Vector2[] locations)
    {
        _id = id;
        _title = title;
        _system = system;
        _arm = arm;
        _locations = locations;
    }

    public string Id => _id;

    public string Title => _title;

    public Vector2 System => _system;

    public Vector2 Arm => _arm;

    public Vector2[] Locations => _locations;
}
