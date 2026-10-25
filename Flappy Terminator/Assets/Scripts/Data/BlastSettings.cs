using System;
using UnityEngine;

[Serializable]
public class BlastSettings
{
    [SerializeField] private float _fuseDelay;
    [SerializeField] private float _radius;
    [SerializeField] private float _time;

    public BlastSettings(float fuseDelay, float radius, float time)
    {
        _fuseDelay = fuseDelay;
        _radius = radius;
        _time = time;
    }

    public float FuseDelay => _fuseDelay;

    public float Radius => _radius;

    public float Time => _time;
}
