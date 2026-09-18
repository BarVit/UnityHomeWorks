using System;
using UnityEngine;

[Serializable]
public class MovementSettings
{
    [SerializeField] private float _driftAmplitude = 1.5f;
    [SerializeField] private float _driftPeriod = 2f;
    [SerializeField] private float _minHoldPositionX = 0f;
    [SerializeField] private float _maxHoldPositionX = 4f;
    [SerializeField] private float _holdDuration = 2f;

    public float DriftAmplitude => _driftAmplitude;

    public float DriftPeriod => _driftPeriod;

    public float MinHoldPositionX => _minHoldPositionX;

    public float MaxHoldPositionX => _maxHoldPositionX;

    public float HoldDuration => _holdDuration;
}
