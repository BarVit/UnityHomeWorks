using System;
using UnityEngine;

public class Economy : MonoBehaviour
{
    [SerializeField] private LocationRunner _runner;

    private SessionProgress _data = new SessionProgress();

    public int Xp => _data.Profile.Xp;

    public int TotalXp => _data.Profile.TotalXp;

    public int Money => _data.Profile.Money;

    public long SpentMoney => _data.Profile.SpentMoney;

    public long SpentXp => _data.Profile.SpentXp;

    public ShipProgress Ship => _data.Ship;

    public LevelBook Skills => _data.Profile.Skills;

    public int CompletedLocations => _data.Galaxy.CompletedLocations.Count;

    public bool IsTestSession { get; private set; }

    public event Action Changed;
    public event Action SessionStarted;
    public event Action<string> LocationCleared;

    private void OnEnable()
    {
        _runner.LevelBanked += Grant;
        _runner.LocationCompleted += CompleteLocation;
    }

    private void OnDisable()
    {
        _runner.LevelBanked -= Grant;
        _runner.LocationCompleted -= CompleteLocation;
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

    public bool IsRequirementMet(string requirement)
    {
        if (string.IsNullOrEmpty(requirement))
            return false;

        if (_runner.Locations.GetRow(requirement) != null)
            return IsLocationCompleted(requirement);

        foreach (LocationRow location in _runner.Locations.GetVoid(requirement))
        {
            if (IsLocationCompleted(location.Id))
                return true;
        }

        return false;
    }

    public void CompleteLocation(string locationId, int stars)
    {
        _data.Galaxy.Complete(locationId, stars);
        LocationCleared?.Invoke(locationId);
    }
}
