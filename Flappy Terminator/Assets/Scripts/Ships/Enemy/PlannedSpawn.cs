public class PlannedSpawn
{
    public const string NoAnnouncement = "";

    public PlannedSpawn(EnemySetup setup, float interval, bool waitsForClearField, string announcement)
    {
        Announcement = announcement;
        Setup = setup;
        Interval = interval;
        WaitsForClearField = waitsForClearField;
    }

    public EnemySetup Setup { get; private set; }

    public float Interval { get; private set; }

    public bool WaitsForClearField { get; private set; }

    public string Announcement { get; private set; }

    public bool HasAnnouncement => string.IsNullOrEmpty(Announcement) == false;
}
