using System;
using UnityEngine;

[Serializable]
public class ProgressionRow
{
    [SerializeField] private string _id;
    [SerializeField] private int _pilotLevel;
    [SerializeField] private int _pilotXpRequired;
    [SerializeField] private int _shipXpRequired;

    public ProgressionRow(string id, int pilotLevel, int pilotXpRequired, int shipXpRequired)
    {
        _id = id;
        _pilotLevel = pilotLevel;
        _pilotXpRequired = pilotXpRequired;
        _shipXpRequired = shipXpRequired;
    }

    public string Id => _id;

    public int PilotLevel => _pilotLevel;

    public int PilotXpRequired => _pilotXpRequired;

    public int ShipXpRequired => _shipXpRequired;
}
