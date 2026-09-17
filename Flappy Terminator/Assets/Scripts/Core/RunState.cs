public class RunState
{
    private const int FirstLevel = 1;

    public int Level { get; private set; } = FirstLevel;

    public int PendingReward { get; private set; }

    public int BankedReward { get; private set; }

    public void Restart()
    {
        Level = FirstLevel;
        PendingReward = 0;
        BankedReward = 0;
    }

    public void RestartRun()
    {
        Level = FirstLevel;
        PendingReward = 0;
    }

    public void AddReward(int amount)
    {
        PendingReward += amount;
    }

    public void DropPending()
    {
        PendingReward = 0;
    }

    public void BankReward()
    {
        BankedReward += PendingReward;
        PendingReward = 0;
    }

    public void AdvanceLevel()
    {
        Level++;
    }
}
