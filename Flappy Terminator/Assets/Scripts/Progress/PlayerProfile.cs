using System;
using UnityEngine;

[Serializable]
public class PlayerProfile
{
    [SerializeField] private int _pilotLevel = 1;
    [SerializeField] private int _pilotXp;
    [SerializeField] private int _money;

    public int PilotLevel => _pilotLevel;

    public int PilotXp => _pilotXp;

    public int Money => _money;

    public void AddXp(int amount)
    {
        _pilotXp += amount;
    }

    public void AddMoney(int amount)
    {
        _money += amount;
    }

    public void SpendXp(int amount)
    {
        _pilotXp -= amount;
        _pilotLevel++;
    }
}
