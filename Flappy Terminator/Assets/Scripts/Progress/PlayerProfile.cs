using System;
using UnityEngine;

[Serializable]
public class PlayerProfile
{
    [SerializeField] private int _xp;
    [SerializeField] private int _totalXp;
    [SerializeField] private int _money;
    [SerializeField] private LevelBook _skills = new LevelBook();

    public int Xp => _xp;

    public int TotalXp => _totalXp;

    public int Money => _money;

    public LevelBook Skills => _skills;

    public void AddXp(int amount)
    {
        _xp += amount;
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

    public bool TrySpendXp(int amount)
    {
        if (amount < 0 || amount > _xp)
            return false;

        _xp -= amount;

        return true;
    }
}
