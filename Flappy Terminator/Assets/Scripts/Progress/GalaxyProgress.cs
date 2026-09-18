using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GalaxyProgress
{
    [SerializeField] private List<string> _completedLocations = new List<string>();

    public IReadOnlyList<string> CompletedLocations => _completedLocations;

    public bool IsCompleted(string locationId)
    {
        return _completedLocations.Contains(locationId);
    }

    public void Complete(string locationId)
    {
        if (_completedLocations.Contains(locationId))
            return;

        _completedLocations.Add(locationId);
    }
}
