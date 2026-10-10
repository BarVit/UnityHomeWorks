using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GalaxyProgress
{
    [SerializeField] private List<string> _completedLocations = new List<string>();

    private readonly Dictionary<string, int> _stars = new Dictionary<string, int>();

    public IReadOnlyList<string> CompletedLocations => _completedLocations;

    public bool IsCompleted(string locationId)
    {
        return _completedLocations.Contains(locationId);
    }

    public int GetStars(string locationId)
    {
        return _stars.TryGetValue(locationId, out int stars) ? stars : 0;
    }

    public void Complete(string locationId, int stars)
    {
        _stars[locationId] = Mathf.Max(stars, GetStars(locationId));

        if (_completedLocations.Contains(locationId))
            return;

        _completedLocations.Add(locationId);
    }
}
