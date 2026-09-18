using System;
using UnityEngine;

[Serializable]
public class SaveData
{
    public const int CurrentVersion = 1;

    [SerializeField] private int _version = CurrentVersion;
    [SerializeField] private PlayerProfile _profile = new PlayerProfile();
    [SerializeField] private ShipProgress _ship = new ShipProgress();
    [SerializeField] private GalaxyProgress _galaxy = new GalaxyProgress();

    public int Version => _version;

    public PlayerProfile Profile => _profile;

    public ShipProgress Ship => _ship;

    public GalaxyProgress Galaxy => _galaxy;
}
