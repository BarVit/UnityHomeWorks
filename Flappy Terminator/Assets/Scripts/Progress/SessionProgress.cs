using System;
using UnityEngine;

[Serializable]
public class SessionProgress
{
    [SerializeField] private PlayerProfile _profile = new PlayerProfile();
    [SerializeField] private ShipProgress _ship = new ShipProgress();
    [SerializeField] private GalaxyProgress _galaxy = new GalaxyProgress();

    public PlayerProfile Profile => _profile;

    public ShipProgress Ship => _ship;

    public GalaxyProgress Galaxy => _galaxy;
}
