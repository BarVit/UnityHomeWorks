using UnityEngine;

public class Economy : MonoBehaviour
{
    [SerializeField] private ProgressionTable _progression;
    [SerializeField] private LocationRunner _runner;

    private SaveData _data;

    public int PilotLevel => _data.Profile.PilotLevel;

    public int PilotXp => _data.Profile.PilotXp;

    public int Money => _data.Profile.Money;

    public int ShipLevel => _data.Ship.ShipLevel;

    public int ShipXp => _data.Ship.ShipXp;

    public int CompletedLocations => _data.Galaxy.CompletedLocations.Count;

    private void Awake()
    {
        _data = SaveStorage.Load();
    }

    private void OnEnable()
    {
        _runner.LevelBanked += Credit;
        _runner.LocationCompleted += Complete;
    }

    private void OnDisable()
    {
        _runner.LevelBanked -= Credit;
        _runner.LocationCompleted -= Complete;
    }

    private void Credit(int kills)
    {
        if (kills <= 0)
            return;

        ProgressionRow pilotRow = _progression.GetRowByLevel(_data.Profile.PilotLevel);
        ProgressionRow shipRow = _progression.GetRowByLevel(_data.Ship.ShipLevel);

        _data.Profile.AddXp(Mathf.RoundToInt(kills * pilotRow.KillXp * pilotRow.LevelBonus));
        _data.Profile.AddMoney(Mathf.RoundToInt(kills * pilotRow.KillMoney * pilotRow.LevelBonus));
        _data.Ship.AddXp(Mathf.RoundToInt(kills * shipRow.KillXp * shipRow.LevelBonus));

        RaisePilotLevel();
        RaiseShipLevel();
    }

    private void Complete(string locationId)
    {
        _data.Galaxy.Complete(locationId);
        SaveStorage.Save(_data);
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
