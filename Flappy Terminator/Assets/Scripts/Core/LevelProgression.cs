using System;
using UnityEngine;

public class LevelProgression : MonoBehaviour
{
    private const int FirstLevel = 1;

    [SerializeField] private LocationTable _locations;
    [SerializeField] private string _locationId;

    private LocationRow _location;
    private int _level = FirstLevel;

    public FlightMode FlightMode => _location.FlightMode;

    public int WaveSize => _location.FirstWaveSize + (_level - FirstLevel) * _location.WaveSizeGrowth;

    public event Action<int> Changed;

    private void Awake()
    {
        _location = _locations.GetRow(_locationId);

        if (_location == null)
            throw new InvalidOperationException(
                "LevelProgression: в таблице локаций нет строки с id " + _locationId);
    }

    public void Restart()
    {
        _level = FirstLevel;
        Changed?.Invoke(_level);
    }

    public void Advance()
    {
        _level++;
        Changed?.Invoke(_level);
    }
}
