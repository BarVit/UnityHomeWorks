using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlarmPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource _alarmSound;

    private float _minVolume = 0;
    private float _maxVolume = 1;
    private float _changeRate = 5;

    void Update()
    {
        
    }
}