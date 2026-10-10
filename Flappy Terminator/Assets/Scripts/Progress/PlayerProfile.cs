using System;
using UnityEngine;

[Serializable]
public class PlayerProfile
{
    [SerializeField] private int _pilotLevel = 1;
    [SerializeField] private int _pilotXp;
    [SerializeField] private int _totalXp;
    [SerializeField] private int _money;

    public int PilotLevel => _pilotLevel;

    public int PilotXp => _pilotXp;

    public int TotalXp => _totalXp;

    public int Money => _money;

    public void AddXp(int amount)
    {
        _pilotXp += amount;
        _totalXp += amount;
    }

    public void AddMoney(int amount)
    {
        _money += amount;
    }

    public bool TrySpendMoney(int amount)
    {
        if (amount < 0 || amount > _money)
            return false;

        _money -= amount;

        return true;
    }

    public void SpendXp(int amount)
    {
        _pilotXp -= amount;
        _pilotLevel++;
    }
}
