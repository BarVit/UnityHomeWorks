using System;
using UnityEngine;

[Serializable]
public class BranchPlacement
{
    [SerializeField] private string _id;
    [SerializeField] private Vector2 _system;
    [SerializeField] private Vector2 _arm;

    public BranchPlacement(string id, Vector2 system, Vector2 arm)
    {
        _id = id;
        _system = system;
        _arm = arm;
    }

    public string Id => _id;

    public Vector2 System => _system;

    public Vector2 Arm => _arm;
}
