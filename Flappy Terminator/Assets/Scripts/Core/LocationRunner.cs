using System;
using UnityEngine;
using UnityEngine.Serialization;

public class LocationRunner : MonoBehaviour
{
    private const int RewardPerKill = 1;

    [SerializeField] private LocationTable _locations;
    [SerializeField] private WaveTable _waves;
    [FormerlySerializedAs("_locationId")]
    [SerializeField] private string _gameLocationId;
    [SerializeField] private string _testLocationId = "loc_test";
    [SerializeField] private EnemySpawner _enemySpawner;

    private readonly RunState _run = new RunState();

    private LocationRow _location;
    private string _locationId;

    public string LocationId => _locationId;

    public FlightMode FlightMode => _location.FlightMode;

    public int Level => _run.Level;

    public int LevelCount => _location.Levels;

    public bool IsLastLevel => _run.Level >= _location.Levels;

    public bool IsTest => _locationId == _testLocationId;

    public bool HasNextLocation => FindNextLocation() != null;

    public int PendingReward => _run.PendingReward;

    public int BankedReward => _run.BankedReward;

    public int WaveSize => _location.FirstWaveSize + (_run.Level - 1) * _location.WaveSizeGrowth;

    public event Action<int> LevelChanged;
    public event Action<int> LevelBanked;
    public event Action<string> LocationCompleted;

    private void Awake()
    {
        Select(_gameLocationId);
    }

    private void OnEnable()
    {
        _enemySpawner.EnemyDied += OnEnemyDied;
    }

    private void OnDisable()
    {
        _enemySpawner.EnemyDied -= OnEnemyDied;
    }

    public void StartGame()
    {
        StartLocation(_gameLocationId);
    }

    public void StartTest()
    {
        StartLocation(_testLocationId);
    }

    public void StartNextLocation()
    {
        LocationRow next = FindNextLocation();

        if (next == null)
            throw new InvalidOperationException(
                "LocationRunner: у локации " + _locationId + " нет следующей локации со сценарием волн");

        Select(next.Id);
        _run.RestartRun();
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

    private void StartLocation(string locationId)
    {
        Select(locationId);
        _run.Restart();
        LevelChanged?.Invoke(_run.Level);
    }

    private void Select(string locationId)
    {
        LocationRow location = _locations.GetRow(locationId);

        if (location == null)
            throw new InvalidOperationException(
                "LocationRunner: в таблице локаций нет строки с id " + locationId);

        _locationId = locationId;
        _location = location;
    }

    private LocationRow FindNextLocation()
    {
        if (IsTest)
            return null;

        LocationRow next = _locations.GetNext(_location);

        if (next == null || _waves.HasLocation(next.Id) == false)
            return null;

        return next;
    }

    private void OnEnemyDied(EnemyShip ship)
    {
        _run.AddReward(RewardPerKill);
    }
}
