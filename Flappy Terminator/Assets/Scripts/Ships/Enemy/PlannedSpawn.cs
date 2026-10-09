public class PlannedSpawn
{
    public PlannedSpawn(EnemySetup setup, float interval, bool waitsForClearField)
    {
        Setup = setup;
        Interval = interval;
        WaitsForClearField = waitsForClearField;
    }

    public EnemySetup Setup { get; private set; }

    public float Interval { get; private set; }

    public bool WaitsForClearField { get; private set; }
}
