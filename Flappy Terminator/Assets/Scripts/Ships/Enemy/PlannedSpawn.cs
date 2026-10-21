public class PlannedSpawn
{
    public const string NoAnnouncement = "";

    public PlannedSpawn(EnemySetup setup, float interval, bool waitsForClearField, string announcement,
        int batchSlot = 0, int batchSize = 1)
    {
        BatchSlot = batchSlot;
        BatchSize = batchSize;
        Announcement = announcement;
        Setup = setup;
        Interval = interval;
        WaitsForClearField = waitsForClearField;
    }

    public EnemySetup Setup { get; private set; }

    public float Interval { get; private set; }

    public bool WaitsForClearField { get; private set; }

    public string Announcement { get; private set; }

    public int BatchSlot { get; private set; }

    public int BatchSize { get; private set; }

    public bool HasAnnouncement => string.IsNullOrEmpty(Announcement) == false;
}
