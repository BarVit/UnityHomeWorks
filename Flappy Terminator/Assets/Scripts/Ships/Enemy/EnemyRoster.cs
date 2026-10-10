using System.Collections.Generic;
using UnityEngine;

public class EnemyRoster
{
    private readonly EnemyTable _enemies;
    private readonly WeaponTable _weapons;
    private readonly LocationTable _locations;
    private readonly SystemTable _systems;
    private readonly List<EnemySetup> _available = new List<EnemySetup>();

    private string _locationId;

    public EnemyRoster(EnemyTable enemies, WeaponTable weapons, LocationTable locations, SystemTable systems)
    {
        _enemies = enemies;
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

        foreach (EnemyRow enemy in _enemies.Rows)
        {
            if (IsOpen(enemy.FromLocation, current))
                _available.Add(EnemySetup.ForEnemy(enemy, _weapons.GetRow(enemy.Weapon)));
        }
    }

    public EnemySetup GetRandom()
    {
        if (_available.Count == 0)
            return null;

        return _available[Random.Range(0, _available.Count)];
    }

    private bool IsOpen(string fromLocation, LocationRow current)
    {
        LocationRow from = _locations.GetRow(fromLocation);

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
