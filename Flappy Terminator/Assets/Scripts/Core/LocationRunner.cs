using System;
using UnityEngine;
using UnityEngine.Serialization;

public class LocationRunner : MonoBehaviour
{
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

    public Reward PendingReward => _run.PendingReward;

    public Reward BankedReward => _run.BankedReward;

    public Reward LocationReward => _run.BankedReward + _run.PendingReward;

    public int WaveSize => GetWaveSize(_run.Level);

    public event Action<int> LevelChanged;
    public event Action<Reward> LevelBanked;
    public event Action<Reward> RewardChanged;
    public event Action<string, int> LocationCompleted;

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
        _run.Restart();
        LevelChanged?.Invoke(_run.Level);
        RewardChanged?.Invoke(LocationReward);
    }

    public int GetWaveSize(int level)
    {
        return _location.FirstWaveSize + (level - 1) * _location.WaveSizeGrowth;
    }

    public void PlayLocation(string locationId)
    {
        StartLocation(locationId);
    }

    public void RetryLocation()
    {
        StartLocation(_locationId);
    }

    public void RestartRun()
    {
        _run.RestartRun();
        LevelChanged?.Invoke(_run.Level);
        RewardChanged?.Invoke(LocationReward);
    }

    public void DropPendingReward()
    {
        _run.DropPending();
        RewardChanged?.Invoke(LocationReward);
    }

    public void BankLevel()
    {
        Reward earned = _run.PendingReward;

        _run.BankReward();
        LevelBanked?.Invoke(earned);
    }

    public void CompleteLocation(int stars)
    {
        LocationCompleted?.Invoke(_locationId, stars);
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
        RewardChanged?.Invoke(LocationReward);
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
        _run.AddReward(ship.Reward);
        RewardChanged?.Invoke(LocationReward);
    }
}
