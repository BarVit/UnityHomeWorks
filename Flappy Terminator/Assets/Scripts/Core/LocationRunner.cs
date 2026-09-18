using System;
using UnityEngine;

public class LocationRunner : MonoBehaviour
{
    private const int RewardPerKill = 1;

    [SerializeField] private LocationTable _locations;
    [SerializeField] private string _locationId;
    [SerializeField] private EnemySpawner _enemySpawner;

    private readonly RunState _run = new RunState();

    private LocationRow _location;

    public string LocationId => _locationId;

    public FlightMode FlightMode => _location.FlightMode;

    public int Level => _run.Level;

    public int LevelCount => _location.Levels;

    public bool IsLastLevel => _run.Level >= _location.Levels;

    public int PendingReward => _run.PendingReward;

    public int BankedReward => _run.BankedReward;

    public int WaveSize => _location.FirstWaveSize + (_run.Level - 1) * _location.WaveSizeGrowth;

    public event Action<int> LevelChanged;
    public event Action<int> LevelBanked;
    public event Action<string> LocationCompleted;

    private void Awake()
    {
        _location = _locations.GetRow(_locationId);

        if (_location == null)
            throw new InvalidOperationException(
                "LocationRunner: в таблице локаций нет строки с id " + _locationId);
    }

    private void OnEnable()
    {
        _enemySpawner.EnemyDied += OnEnemyDied;
    }

    private void OnDisable()
    {
        _enemySpawner.EnemyDied -= OnEnemyDied;
    }

    public void StartLocation()
    {
        _run.Restart();
        LevelChanged?.Invoke(_run.Level);
    }

    public void RestartRun()
    {
        _run.RestartRun();
        LevelChanged?.Invoke(_run.Level);
    }

    public void DropPendingReward()
    {
        _run.DropPending();
    }

    public void BankLevel()
    {
        int earned = _run.PendingReward;

        _run.BankReward();
        LevelBanked?.Invoke(earned);
    }

    public void CompleteLocation()
    {
        LocationCompleted?.Invoke(_locationId);
    }

    public void Advance()
    {
        _run.AdvanceLevel();
        LevelChanged?.Invoke(_run.Level);
    }

    private void OnEnemyDied(EnemyShip ship)
    {
        _run.AddReward(RewardPerKill);
    }
}
