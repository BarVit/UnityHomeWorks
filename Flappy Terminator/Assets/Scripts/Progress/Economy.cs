using System;
using UnityEngine;

public class Economy : MonoBehaviour
{
    [SerializeField] private LocationRunner _runner;

    private SessionProgress _data = new SessionProgress();

    public int Xp => _data.Profile.Xp;

    public int TotalXp => _data.Profile.TotalXp;

    public int Money => _data.Profile.Money;

    public ShipProgress Ship => _data.Ship;

    public LevelBook Skills => _data.Profile.Skills;

    public int CompletedLocations => _data.Galaxy.CompletedLocations.Count;

    public bool IsTestSession { get; private set; }

    public event Action Changed;
    public event Action SessionStarted;

    private void OnEnable()
    {
        _runner.LevelBanked += Grant;
        _runner.LocationCompleted += Complete;
    }

    private void OnDisable()
    {
        _runner.LevelBanked -= Grant;
        _runner.LocationCompleted -= Complete;
    }

    public bool IsLocationCompleted(string locationId)
    {
        return _data.Galaxy.IsCompleted(locationId);
    }

    public int GetStars(string locationId)
    {
        return _data.Galaxy.GetStars(locationId);
    }

    public bool TrySpend(int price)
    {
        if (_data.Profile.TrySpendMoney(price) == false)
            return false;

        Changed?.Invoke();

        return true;
    }

    public bool TrySpendXp(int price)
    {
        if (_data.Profile.TrySpendXp(price) == false)
            return false;

        Changed?.Invoke();

        return true;
    }

    public void StartNewSession(bool isTest)
    {
        IsTestSession = isTest;
        _data = new SessionProgress();
        SessionStarted?.Invoke();
        Changed?.Invoke();
    }

    public void Grant(Reward reward)
    {
        if (reward.IsEmpty)
            return;

        _data.Profile.AddXp(reward.Xp);
        _data.Profile.AddMoney(reward.Money);
        Changed?.Invoke();
    }

    private void Complete(string locationId, int stars)
    {
        _data.Galaxy.Complete(locationId, stars);
    }
}
