using System;
using UnityEngine;

[Serializable]
public class LocationPositions
{
    [SerializeField] private Vector2[] _points;

    public LocationPositions(Vector2[] points)
    {
        _points = points;
    }

    public Vector2[] Points => _points;
}
