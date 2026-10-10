using System;
using UnityEngine;

public class Economy : MonoBehaviour
{
    [SerializeField] private ProgressionTable _progression;
    [SerializeField] private LocationRunner _runner;

    private SessionProgress _data = new SessionProgress();

    public int PilotLevel => _data.Profile.PilotLevel;

    public int PilotXp => _data.Profile.PilotXp;

    public int TotalXp => _data.Profile.TotalXp;

    public int Money => _data.Profile.Money;

    public int ShipLevel => _data.Ship.ShipLevel;

    public int ShipXp => _data.Ship.ShipXp;

    public ShipProgress Ship => _data.Ship;

    public int CompletedLocations => _data.Galaxy.CompletedLocations.Count;

    public event Action Changed;

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

    public bool TrySpend(int price)
    {
        if (_data.Profile.TrySpendMoney(price) == false)
            return false;

        Changed?.Invoke();

        return true;
    }

    public void StartNewSession()
    {
        _data = new SessionProgress();
        Changed?.Invoke();
    }

    public void Grant(Reward reward)
    {
        if (reward.IsEmpty)
            return;

        _data.Profile.AddXp(reward.Xp);
        _data.Profile.AddMoney(reward.Money);
        _data.Ship.AddXp(reward.Xp);

        RaisePilotLevel();
        RaiseShipLevel();
        Changed?.Invoke();
    }

    private void Complete(string locationId)
    {
        _data.Galaxy.Complete(locationId);
    }

    private void RaisePilotLevel()
    {
        ProgressionRow row = _progression.GetRowByLevel(_data.Profile.PilotLevel);

        while (row.PilotXpRequired > 0 && _data.Profile.PilotXp >= row.PilotXpRequired)
        {
            _data.Profile.SpendXp(row.PilotXpRequired);

            ProgressionRow next = _progression.GetRowByLevel(_data.Profile.PilotLevel);

            if (next == row)
                return;

            row = next;
        }
    }

    private void RaiseShipLevel()
    {
        ProgressionRow row = _progression.GetRowByLevel(_data.Ship.ShipLevel);

        while (row.ShipXpRequired > 0 && _data.Ship.ShipXp >= row.ShipXpRequired)
        {
            _data.Ship.SpendXp(row.ShipXpRequired);

            ProgressionRow next = _progression.GetRowByLevel(_data.Ship.ShipLevel);

            if (next == row)
                return;

            row = next;
        }
    }
}
