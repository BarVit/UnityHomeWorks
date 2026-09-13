using System;
using UnityEngine;

public class LevelProgression : MonoBehaviour
{
    private const int FirstLevel = 1;

    [SerializeField] private int _firstWaveSize = 6;
    [SerializeField] private int _waveSizeGrowth = 3;

    private int _level = FirstLevel;

    public int WaveSize => _firstWaveSize + (_level - FirstLevel) * _waveSizeGrowth;

    public event Action<int> Changed;

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
