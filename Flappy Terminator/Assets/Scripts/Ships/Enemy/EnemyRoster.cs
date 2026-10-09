using System.Collections.Generic;
using UnityEngine;

public class EnemyRoster
{
    private readonly EnemyShipTable _ships;
    private readonly EnemyTierTable _tiers;
    private readonly WeaponTable _weapons;
    private readonly LocationTable _locations;
    private readonly SystemTable _systems;
    private readonly List<EnemySetup> _available = new List<EnemySetup>();

    private string _locationId;

    public EnemyRoster(EnemyShipTable ships, EnemyTierTable tiers, WeaponTable weapons,
        LocationTable locations, SystemTable systems)
    {
        _ships = ships;
        _tiers = tiers;
        _weapons = weapons;
        _locations = locations;
        _systems = systems;
    }

    public IReadOnlyList<EnemySetup> Available => _available;

    public void Open(string locationId)
    {
        if (_locationId == locationId)
            return;

        _locationId = locationId;
        _available.Clear();

        LocationRow current = _locations.GetRow(locationId);

        if (current == null)
            return;

        foreach (EnemyShipRow ship in _ships.Rows)
        {
            if (IsOpen(ship, current) == false)
                continue;

            foreach (EnemyTierRow tier in _tiers.GetTiers(ship.Id))
                _available.Add(new EnemySetup(ship, tier, _weapons.GetRow(tier.Weapon)));
        }
    }

    public EnemySetup GetRandom()
    {
        if (_available.Count == 0)
            return null;

        return _available[Random.Range(0, _available.Count)];
    }

    private bool IsOpen(EnemyShipRow ship, LocationRow current)
    {
        LocationRow from = _locations.GetRow(ship.FromLocation);

        if (from == null)
            return false;

        int fromSystem = GetSystemOrder(from.System);
        int currentSystem = GetSystemOrder(current.System);

        if (fromSystem != currentSystem)
            return fromSystem < currentSystem;

        return from.Order <= current.Order;
    }

    private int GetSystemOrder(string systemId)
    {
        SystemRow row = _systems.GetRow(systemId);

        return row == null ? int.MaxValue : row.Order;
    }
}
