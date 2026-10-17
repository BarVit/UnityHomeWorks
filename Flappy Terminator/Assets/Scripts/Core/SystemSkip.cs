using System.Collections.Generic;
using UnityEngine;

public class SystemSkip : MonoBehaviour
{
    private const int MaxStars = 3;

    [SerializeField] private Economy _economy;
    [SerializeField] private LocationTable _locations;
    [SerializeField] private SystemTable _systems;
    [SerializeField] private WaveTable _waves;
    [SerializeField] private EnemySpawner _enemySpawner;

    public Reward Complete(string locationId, Reward alreadyBanked, out string nextLocationId)
    {
        LocationRow current = GetRoutePoint(_locations.GetRow(locationId));
        Reward earned = default;

        foreach (LocationRow location in GetRoute(current.System))
        {
            if (_economy.IsLocationCompleted(location.Id))
                continue;

            earned += _enemySpawner.CountReward(location.Id, location.Levels);

            if (location.Id == locationId)
                earned -= alreadyBanked;

            _economy.CompleteLocation(location.Id, MaxStars);
        }

        _economy.Grant(earned);
        nextLocationId = FindNextSystemStart(current.System);

        return earned;
    }

    private string FindNextSystemStart(string systemId)
    {
        SystemRow system = _systems.GetRow(systemId);

        foreach (SystemRow next in _systems.Rows)
        {
            if (next.Order != system.Order + 1)
                continue;

            List<LocationRow> route = GetRoute(next.Id);

            return route.Count > 0 && _waves.IsPlayable(route[0]) ? route[0].Id : null;
        }

        return null;
    }

    private List<LocationRow> GetRoute(string systemId)
    {
        List<LocationRow> route = new List<LocationRow>();

        foreach (LocationRow location in _locations.Rows)
        {
            if (location.System == systemId && location.IsBranch == false)
                route.Add(location);
        }

        route.Sort((left, right) => left.Order.CompareTo(right.Order));

        return route;
    }

    private LocationRow GetRoutePoint(LocationRow location)
    {
        return location.IsBranch ? _locations.GetRow(location.BranchOf) : location;
    }
}
