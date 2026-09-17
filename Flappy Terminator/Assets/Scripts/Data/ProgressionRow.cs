using System;
using UnityEngine;

[Serializable]
public class ProgressionRow
{
    [SerializeField] private string _id;
    [SerializeField] private int _pilotLevel;
    [SerializeField] private int _pilotXpRequired;
    [SerializeField] private int _shipXpRequired;
    [SerializeField] private int _killXp;
    [SerializeField] private int _killMoney;
    [SerializeField] private float _levelBonus;

    public ProgressionRow(string id, int pilotLevel, int pilotXpRequired, int shipXpRequired,
        int killXp, int killMoney, float levelBonus)
    {
        _id = id;
        _pilotLevel = pilotLevel;
        _pilotXpRequired = pilotXpRequired;
        _shipXpRequired = shipXpRequired;
        _killXp = killXp;
        _killMoney = killMoney;
        _levelBonus = levelBonus;
    }

    public string Id => _id;

    public int PilotLevel => _pilotLevel;

    public int PilotXpRequired => _pilotXpRequired;

    public int ShipXpRequired => _shipXpRequired;

    public int KillXp => _killXp;

    public int KillMoney => _killMoney;

    public float LevelBonus => _levelBonus;
}
